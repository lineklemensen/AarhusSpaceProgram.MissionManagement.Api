namespace AarhusSpaceProgram.MissionManagement.Api.Middleware
{
    public sealed class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _asplogger;

        public HttpLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _asplogger = loggerFactory.CreateLogger("ASPMiddleware.HttpLogs");
        }

        public async Task Invoke(HttpContext context)
        {
            var method = context.Request.Method;

            var isWrite =
                HttpMethods.IsPost(method) ||
                HttpMethods.IsPut(method) ||
                HttpMethods.IsPatch(method) ||
                HttpMethods.IsDelete(method);

            if (isWrite)
            {
                await _next(context);
                return;
            }

            await _next(context);

            _asplogger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode}",
                method,
                context.Request.Path.Value,
                context.Response.StatusCode);
        }
    }
}
