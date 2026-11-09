using MiniApi.Data;
using MiniApi.Extensions;

public partial class Program
{
    
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // logging
        builder.AddAppLogging();
        
        // servicios (Swagger, HealthChecks, Dapper, etc.)
        builder.Services.AddAppServices(); 
        var app = builder.Build();
        
        // inicializar bd
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
        
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError("OK");
        
        // middleware (Swagger UI, Serilog, HealthChecks)
        app.UseAppMiddleware(); 
        
        // endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}