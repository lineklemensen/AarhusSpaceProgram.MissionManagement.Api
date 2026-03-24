using Microsoft.AspNetCore.Http;

namespace AarhusSpaceProgram.MissionManagement.Api.Logging
{
    public static class AuditContext
    {
        public const string ActionKey = "Audit.Action";
        public const string RequestDataKey = "Audit.RequestData";
        public const string ResponseDataKey = "Audit.ResponseData";

        public static void SetAction(HttpContext http, string action) => http.Items[ActionKey] = action;

        public static void SetRequestData(HttpContext http, object request) => http.Items[RequestDataKey] = request;

        public static void SetResponseData(HttpContext http, object response) => http.Items[ResponseDataKey] = response;
    }
}
