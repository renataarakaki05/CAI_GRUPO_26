using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Products.API.Extensions
{
    public static class LoggingExtensions
    {
        public const string ServiceName = "Products.API";

        // Se llama antes que cualquier otra configuración en Program.cs
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Servicio", ServiceName)

                // Consola: formato legible
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Endpoint} {Message:lj}{NewLine}{Exception}")

                // Archivo: JSON estructurado (Timestamp, Nivel, Servicio, Endpoint, CorrelationId, ErrorCode...)
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/products-.json",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
