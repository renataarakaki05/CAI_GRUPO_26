using Notifications.API.Data;
using Notifications.API.Extensions;

public partial class Program
{

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); // es como el canva

        // 1. Logging (Serilog va antes que cualquier otra configuración)
        builder.AddAppLogging();

        // 2. Servicios (Swagger, HealthChecks, Dapper, ExceptionHandlers, HttpClient)
        builder.Services.AddAppServices(builder.Configuration);
        var app = builder.Build();

        // 3. Inicializar DB
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

        // 4. Middleware (Correlation ID, Serilog, Auditoría, ExceptionHandler, Swagger UI, HealthChecks)
        app.UseAppMiddleware();

        // 5. Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}
