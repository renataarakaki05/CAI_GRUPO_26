using Products.API.Data;
using Products.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog va antes que cualquier otra configuración)
builder.AddAppLogging();

try
{
    // 2. Servicios (controllers, exception handlers, Swagger, HealthChecks, HttpClient)
    builder.Services.AddAppServices(builder.Configuration);
    var app = builder.Build();

    // 3. Inicializar DB
    using (var scope = app.Services.CreateScope())
        scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

    // 4. Middleware y endpoints (Correlation ID, UseExceptionHandler, Swagger, Health, Controllers)
    app.UseAppMiddleware();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Products.API no pudo iniciar");
}
finally
{
    Log.CloseAndFlush();
}

// Necesario para poder referenciar Program desde tests
public partial class Program { }
