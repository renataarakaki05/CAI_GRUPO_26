using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Middleware;
using Cart.API.Services;
using Cart.API.Swagger;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Swashbuckle.AspNetCore.SwaggerGen;

public partial class Program
{

    private static void Main(string[] args)
    {
        // Cosas q vamos a usar en la API
        var builder = WebApplication.CreateBuilder(args);

        // los logs van primero q todo, asi se loguea desde el arranque
        ConfigurarLogs();
        builder.Host.UseSerilog();

        // usamo addControllers donde se guarda los endpoints
        builder.Services.AddControllers();

        // los datos los autovalidamos nosotros con CRT-004
        builder.Services.Configure<ApiBehaviorOptions>(ConfigurarValidacion);

        // El manjeo de errores buscar ir del mas especifico al mas general
        // Cuando salta una excepción, .NET prueba los handlers en este orden hasta que uno la agarra.
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // base de datos: el q crea las tablas y el repositorio con las consultas
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddScoped<CarritoRepository>();

        // cliente http para consultar products.api
        // el handler le agrega el correlation id a cada pedido q sale
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
        builder.Services.AddHttpClient<ProductsClient>().AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        // la logica del carrito
        builder.Services.AddScoped<CarritoService>();

        // Esto es para que Swagger pueda leer los endpoints y armar la página de prueba
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(ConfigurarSwagger);

        // health checks: la base, la api y products (cada uno con su etiqueta)
        builder.Services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", null, new string[] { "database" })
            .AddCheck<ApiStatusCheck>("api-status", null, new string[] { "api" })
            .AddCheck<ProductsApiHealthCheck>("products-api", null, new string[] { "dependencias" });

        // el panel visual de los health checks (/health-ui)
        builder.Services.AddHealthChecksUI(ConfigurarHealthUI).AddInMemoryStorage();

        // Con todo configurado, armamos la app
        var app = builder.Build();

        // creamos las tablas si no existen
        using (var scope = app.Services.CreateScope())
        {
            var inicializador = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            inicializador.Initialize();
        }

        // primero el correlation id, asi todos los logs del request lo tienen
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Swagger solo se muestra cuando estamos desarrollando, no en producción
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // loguea el fin de cada request con el status y cuanto tardo
        app.UseSerilogRequestLogging(ConfigurarLogDeRequests);

        // Activa los handlers de errores que registramos arriba
        app.UseExceptionHandler();

        // /health todo, /health/ready la base y products, /health/live solo la api
        app.MapHealthChecks("/health", CrearOpcionesHealth(null));
        app.MapHealthChecks("/health/ready", CrearOpcionesHealth(EsCheckReady));
        app.MapHealthChecks("/health/live", CrearOpcionesHealth(EsCheckLive));
        app.MapHealthChecksUI(ConfigurarRutaHealthUI);

        // Conecta las rutas  con los métodos de los Controllers
        app.MapControllers();

        // Prende la API y se queda escuchando pedidos
        app.Run();
    }


    // para q asp.net no corte con su 400 y el error lo manejemos nosotros
    private static void ConfigurarValidacion(ApiBehaviorOptions options)
    {
        options.SuppressModelStateInvalidFilter = true;

    }

    // El swagger lee los comentarios del codigo y agrega los ejemplos de error
    private static void ConfigurarSwagger(SwaggerGenOptions options)
    {
        var archivoXml = Path.Combine(AppContext.BaseDirectory, "Cart.API.xml");
        options.IncludeXmlComments(archivoXml);
        options.OperationFilter<ErrorExamplesOperationFilter>();
    }

    // Serilogs

    // consola: legible para leer mientras programamos
    // logs/cart.json: json con todos los datos (timestamp, nivel, servicio, endpoint, correlation id, errorCode)
    // logs/audit.log: una linea por request de negocio
    private static void ConfigurarLogs()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", "Cart.API")
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Endpoint} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(new CompactJsonFormatter(), "logs/cart.json", rollingInterval: RollingInterval.Day)
            .WriteTo.Logger(ConfigurarLogDeAuditoria)
            .CreateLogger();
    }

    // el audit.log solo guarda los fines de request de los endpoints de negocio (no health ni swagger)
    private static void ConfigurarLogDeAuditoria(LoggerConfiguration config)
    {
        config
            .Filter.ByIncludingOnly(EsLogDeAuditoria)
            .WriteTo.File(
                "logs/audit.log",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {CorrelationId} | {RequestMethod} | {RequestPath} | {StatusCode} | {Elapsed:0} ms{NewLine}",
                rollingInterval: RollingInterval.Day);
    }

    // devuelve true si el log es el fin de un request de negocio
    private static bool EsLogDeAuditoria(LogEvent evento)
    {
        // solo los logs q escribe UseSerilogRequestLogging (uno por request, al final)
        if (!evento.Properties.ContainsKey("SourceContext") || !evento.Properties.ContainsKey("RequestPath"))
        {
            return false;
        }

        var origen = evento.Properties["SourceContext"].ToString();
        if (!origen.Contains("RequestLoggingMiddleware"))
        {
            return false;
        }

        // los health y swagger no nos interesan
        var ruta = evento.Properties["RequestPath"].ToString();
        if (ruta.Contains("/health") || ruta.Contains("/swagger"))
        {
            return false;
        }

        return true;
    }

    private static void ConfigurarLogDeRequests(RequestLoggingOptions options)
    {
        options.GetLevel = NivelDelLog;
    }

    // errores inesperados como Error, los de negocio (4xx) como Warning, lo demas Information
    // los health checks no ensucian el log
    private static LogEventLevel NivelDelLog(HttpContext context, double duracion, Exception? ex)
    {
        if (ex != null || context.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return LogEventLevel.Verbose;
        }

        if (context.Response.StatusCode >= 400)
        {
            return LogEventLevel.Warning;
        }

        return LogEventLevel.Information;
    }

    // Health Checks

    // arma las opciones de cada endpoint de health, con el filtro q le pasemos
    private static HealthCheckOptions CrearOpcionesHealth(Func<HealthCheckRegistration, bool>? filtro)
    {
        var opciones = new HealthCheckOptions();
        opciones.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;
        if (filtro != null)
        {
            opciones.Predicate = filtro;
        }

        return opciones;
    }

    // ready: la base y products (lo q necesitamos para atender pedidos)
    private static bool EsCheckReady(HealthCheckRegistration check)
    {
        return check.Tags.Contains("database") || check.Tags.Contains("dependencias");
    }

    // live: solo q el proceso este vivo
    private static bool EsCheckLive(HealthCheckRegistration check)
    {
        return check.Tags.Contains("api");
    }

    private static void ConfigurarHealthUI(Settings settings)
    {
        settings.SetEvaluationTimeInSeconds(600);
        settings.AddHealthCheckEndpoint("Cart.API", "/health");
    }

    private static void ConfigurarRutaHealthUI(Options options)
    {
        options.UIPath = "/health-ui";
    }
}