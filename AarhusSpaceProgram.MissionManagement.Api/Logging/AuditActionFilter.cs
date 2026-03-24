using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AarhusSpaceProgram.MissionManagement.Api.Logging;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.ActionDescriptor.RouteValues["controller"] ?? "Unknown";
        var action = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";

        // Fx "CelestialBodies.Create" / "CelestialBodies.Update" / "CelestialBodies.Delete"
        var op = action switch
        {
            "Post" => "Create",
            "Put" or "Patch" => "Update",
            "Delete" => "Delete",
            _ => action
        };

        AuditContext.SetAction(context.HttpContext, $"{controller}.{op}");

        var executed = await next();

        // Default: ingen payload
        object? responseSlim = null;

        // Prøv at hente ObjectResult.Value (som i dit eksempel er RestDTO<T>)
        if (executed.Result is ObjectResult o && o.Value is not null)
        {
            // Prøv at finde "Data" property på wrapperen (RestDTO<T>)
            var dataProp = o.Value.GetType().GetProperty("Data");
            var data = dataProp?.GetValue(o.Value);

            // Prøv at udtrække Id og Name fra Data
            if (data is not null)
            {
                var idProp = data.GetType().GetProperty("Id");
                var nameProp = data.GetType().GetProperty("Name");

                var id = idProp?.GetValue(data);
                var name = nameProp?.GetValue(data);

                // Kun log hvis vi fandt mindst ét af dem
                if (id is not null || name is not null)
                {
                    responseSlim = new { id, name };
                }
            }
        }

        // (Valgfrit) hvis delete typisk ikke returnerer Data, kan du tage id fra route:
        if (responseSlim is null && context.RouteData.Values.TryGetValue("id", out var routeId))
        {
            responseSlim = new { id = routeId };
        }

        AuditContext.SetResponseData(context.HttpContext, responseSlim);

        // Drop request helt (for at fjerne støj)
        AuditContext.SetRequestData(context.HttpContext, null!);
    }
}