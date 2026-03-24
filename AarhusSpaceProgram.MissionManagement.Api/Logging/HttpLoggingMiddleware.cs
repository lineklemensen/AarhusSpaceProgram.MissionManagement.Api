namespace AarhusSpaceProgram.MissionManagement.Api.Logging
{
    public sealed class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _asplogger;

        public HttpLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _asplogger = loggerFactory.CreateLogger("ASP.HttpLogs");
        }

        public async Task Invoke(HttpContext context)
        {
            var method = context.Request.Method;

            var isWrite =
                HttpMethods.IsPost(method) ||
                HttpMethods.IsPut(method) ||
                HttpMethods.IsPatch(method) ||
                HttpMethods.IsDelete(method);

            if (!isWrite)
            {
                await _next(context);
                return;
            }

            await _next(context);

            context.Items.TryGetValue(AuditContext.ActionKey, out var actionObj);
            context.Items.TryGetValue(AuditContext.RequestDataKey, out var requestObj);
            context.Items.TryGetValue(AuditContext.ResponseDataKey, out var responseObj);

            _asplogger.LogInformation(
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} action={Action} request={@Request} response={@Response}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Request.QueryString.Value,
                context.Response.StatusCode,
                actionObj,
                requestObj,
                responseObj);
        }
    }
}
