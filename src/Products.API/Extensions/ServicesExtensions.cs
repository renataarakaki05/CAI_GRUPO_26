using System.Reflection;
using Products.API.Clients;
using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Middleware;
using Products.API.Services;
using Products.API.Swagger;

namespace Products.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<ProductoRepository>();
            services.AddScoped<ProductoService>();

            // manejo de errores 
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

          
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

            
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddHttpClient<OrdersClient>(client =>
            {
                client.BaseAddress = new Uri(config["Services:OrdersApi"] ?? "http://localhost:5103");
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
                setup.AddHealthCheckEndpoint("Products.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
