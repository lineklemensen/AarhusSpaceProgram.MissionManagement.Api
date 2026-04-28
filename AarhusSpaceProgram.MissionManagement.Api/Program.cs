using AarhusSpaceProgram.MissionManagement.Api.Logging;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AarhusSpaceProgram.MissionManagement.Api.Services;
using AarhusSpaceProgram.MissionManagement.Api.Infrastructure;
using Microsoft.OpenApi;
using MongoDB.Driver;
using AarhusSpaceProgram.Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((cxt, svc, lc) =>
{
    lc.ReadFrom.Configuration(cxt.Configuration);
});

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        });
});

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The fiels '{x}' must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is invalid for the field '{y}'.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "A value is required.");

    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddScoped<AarhusSpaceProgram.MissionManagement.Api.Logging.AuditActionFilter>();

builder.Services.AddScoped<StaffService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, CancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter your token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<MissionManagementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
    );

builder.Services.AddSingleton<IMongoCollection<MissionLogDTO>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var connectionString = config["Mongo:ConnectionString"];
    var databaseName = config["Mongo:DatabaseName"];
    var collectionName = config["Mongo:MissionLogsCollectionName"];

    var client = new MongoClient(connectionString);
    var db = client.GetDatabase(databaseName);

    return db.GetCollection<MissionLogDTO>(collectionName);
});

// Identity service configuration
builder.Services.AddIdentity<AspUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<MissionManagementDbContext>();

// Authentication service configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:SigningKey"]))
    };
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);

builder.Services.AddScoped<SeedData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AarhusSpaceProgram.MissionManagement.Api.Logging.HttpLoggingMiddleware>();

app.MapGet("/error", 
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () => 
    Results.Problem());

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedData>();
    await seeder.SeedAsync();
}

    app.Run();
