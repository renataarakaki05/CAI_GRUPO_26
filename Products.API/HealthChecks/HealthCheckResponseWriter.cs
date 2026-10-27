using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.HealthChecks
{
    // Respuesta JSON de /health, /health/ready y /health/live con estado Healthy, Degraded o Unhealthy.
    public static class HealthCheckResponseWriter
    {
        public static Task WriteAsync(HttpContext context, HealthReport report)
        {
            var body = new
            {
                servicio = "Products.API",
                status = report.Status.ToString(),
                duracionMs = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(e => new
                {
                    nombre = e.Key,
                    status = e.Value.Status.ToString(),
                    descripcion = e.Value.Description
                })
            };

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
    }
}
