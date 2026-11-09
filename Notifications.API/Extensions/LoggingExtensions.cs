using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace Notifications.API.Extensions
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
                .Enrich.WithProperty("Servicio", "Notifications.API")

               
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Endpoint} {Message:lj}{NewLine}{Exception}")

               
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/notifications.json",
                    rollingInterval: RollingInterval.Day)

               
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                       
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
