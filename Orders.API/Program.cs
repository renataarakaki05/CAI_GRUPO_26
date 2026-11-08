using Orders.API.Data;
using Orders.API.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Logging
        builder.AddAppLogging();

        // Servicios (bd, ss de órdenes, handlers de errores, clientes HTTP, swagger, healthchecks)
        builder.Services.AddAppServices(builder.Configuration);
        var app = builder.Build();

        // Inicializar DB
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

        // Middleware (correlation ID, swagger UI, serilog, auditoría, handlers de errores, healthchecks)
        app.UseAppMiddleware();

        // Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}