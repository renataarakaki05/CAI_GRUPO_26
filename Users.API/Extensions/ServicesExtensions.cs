using Users.API.Swagger;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.HealthChecks;
using Users.API.Services;

namespace Users.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<UsuarioRepository>();
            services.AddScoped<UsuarioService>();

            // Handlers de errores: primero los específicos y al final el general
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Controllers, los datos los validamos nosotros en el servicio, USR-002
            services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"; // Lee  comentarios XML de los DTOs
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

                options.OperationFilter<ErrorExamplesFilter>(); // Agrega ejemplos de error de cada endpoint
            });

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Users.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
