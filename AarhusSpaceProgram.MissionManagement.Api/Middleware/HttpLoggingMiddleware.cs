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

    }
}
