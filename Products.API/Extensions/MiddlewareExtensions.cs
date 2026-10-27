using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Products.API.HealthChecks;
using Products.API.Middleware;

namespace Products.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // Primero el Correlation ID, así todos los logs (incluidos los de errores) lo tienen
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseExceptionHandler();

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Products.API v1"));

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapControllers();
        }
    }
}
