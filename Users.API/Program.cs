using Users.API.Data;
using Users.API.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Logging
        builder.AddAppLogging();

        // Servicios (bd, ss de usuarios, handlers de errores, swagger, healthchecks)
        builder.Services.AddAppServices();
        var app = builder.Build();

        // Inicia bd
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

        // Middleware (correlation id, swagger ui, serilog, auditoria, handlers de errores, healthchecks)
        app.UseAppMiddleware();

        // Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}