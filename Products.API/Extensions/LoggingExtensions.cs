using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace Products.API.Extensions
{
    public static class LoggingExtensions
    {
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Servicio", "Products.API")

                // ALUMNO: LOG DE CONSOLA: formato legible, con servicio, Correlation ID y endpoint
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Endpoint} {Message:lj}{NewLine}{Exception}")

                // ALUMNO: LOG DE ARCHIVO: JSON estructurado (Timestamp, Nivel, Servicio, Endpoint, CorrelationId, ErrorCode)
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/products.json",
                    rollingInterval: RollingInterval.Day)

                // ALUMNO: LOG DE AUDITORÍA: Solo registra cuando se consume un endpoint, es una auditoría limpia.
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        // Solo aceptar logs que vengan del Middleware de Serilog
                        // Esto elimina automáticamente los duplicados de Microsoft.AspNetCore.Mvc
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                        // Excluir rutas irrelevantes
                        if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                            pathValue is ScalarValue scalar && scalar.Value is string path)
                        {
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
                        }

                        return true;
                    })
                    .WriteTo.File(
                        path: "logs/audit.log",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {CorrelationId} | {RequestMethod} | {RequestPath} | {StatusCode} | {Elapsed:0} ms{NewLine}",
                        rollingInterval: RollingInterval.Day))
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
