using Orders.API.Clients;
using Orders.API.Data;
using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Middleware;
using Orders.API.Services;
using Orders.API.Swagger;

namespace Orders.API.Extensions
{

    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<OrdenRepository>();
            services.AddScoped<OrdenService>();

            // Handlers de errores primero los específicos y al final el general
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Clientes para llamar a Users.API y Products.API.
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdHandler>();

            services.AddHttpClient<UsersClient>(client =>
            {
                client.BaseAddress = new Uri(config["Services:UsersApi"] ?? "http://localhost:5102");
            }).AddHttpMessageHandler<CorrelationIdHandler>();

            services.AddHttpClient<ProductsClient>(client =>
            {
                client.BaseAddress = new Uri(config["Services:ProductsApi"] ?? "http://localhost:5101");
            }).AddHttpMessageHandler<CorrelationIdHandler>();

            // Controllers. Los datos los validamos nosotros en el servicio (error ORD-002)
            services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"; // Lee los comentarios XML de los DTOs y del controller 
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

                options.OperationFilter<ErrorExamplesFilter>();  // Agrega los ejemplos de error de cada endpoint
            });

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Orders.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
