using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Notifications.API.Clients;
using Notifications.API.Data;
using Notifications.API.ExceptionHandlers;
using Notifications.API.HealthChecks;
using Notifications.API.Middleware;
using Notifications.API.Services;
using Notifications.API.Swagger;

namespace Notifications.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<NotificacionRepository>();
            services.AddScoped<NotificacionService>();

            // Manejo de errores: del más específico al genérico (Apéndice B)
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Un body mal formado lanza excepción en todos los entornos → ValidationExceptionHandler (NTF-002)
            services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

            // HTTP hacia Users.API (IHttpClientFactory), propagando el X-Correlation-Id
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddHttpClient<UsersClient>(client =>
            {
                client.BaseAddress = new Uri(config["Services:UsersApi"] ?? "http://localhost:5102");
                client.Timeout = TimeSpan.FromSeconds(5);
            }).AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Comentarios XML de los DTOs y ejemplos de error del catálogo
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
                options.OperationFilter<ErrorExamplesOperationFilter>();
            });

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Notifications.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
