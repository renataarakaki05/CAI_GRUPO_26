using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace Orders.API.Extensions
{
    public static class LoggingExtensions
    {
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning) // mensajes internos del HttpClient
                .MinimumLevel.Override("HealthChecks", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Servicio", "Orders.API")

                // LOG DE CONSOLA 
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")

                // LOG DE ARCHIVO JSON estructurado 
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/orders.json",
                    rollingInterval: RollingInterval.Day)

                // LOG DE AUDITORÍA solo registra cuando se consume un endpoint
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        // Solo aceptar logs que vengan del Middleware de Serilog
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