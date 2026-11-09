using System.Reflection;
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

            // Manejo de errores, del más específico al genérico
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

            
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
