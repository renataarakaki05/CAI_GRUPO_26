using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API.Clients;
using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.Filters;
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
            // Persistencia y lógica de negocio
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<ProductoRepository>();
            services.AddScoped<ProductoService>();

            // Controllers + validación con Data Annotations → PRD-002
            services.AddControllers(options => options.Filters.Add<ValidateModelFilter>());
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            // Manejo de errores: del más específico al genérico
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // HTTP hacia Orders.API, propagando el X-Correlation-Id
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddHttpClient<OrdersClient>(client =>
            {
                client.BaseAddress = new Uri(config["Services:OrdersApi"] ?? "http://localhost:5103");
                client.Timeout = TimeSpan.FromSeconds(5);
            }).AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            // Swagger con comentarios XML y ejemplos de error
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "Products.API", Version = "v1", Description = "Microservicio de productos del E-Commerce." });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
                options.OperationFilter<ErrorExamplesOperationFilter>();
            });

            // Health checks: "ready" verifica la base; "live" solo que el proceso responde
            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);
        }
    }
}
