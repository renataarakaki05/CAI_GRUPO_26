using Cart.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;

public partial class Program
{
   
    private static void Main(string[] args)
    {
        // Cosas q vamos a usar en la API
        var builder = WebApplication.CreateBuilder(args);

        // usamo addControllers donde se guarda los endpoints
        builder.Services.AddControllers();

        // Apagamos el 400 automático de ASP.NET: los datos los autovalidamos nosotros con CRT-004
        builder.Services.Configure<ApiBehaviorOptions>(ConfigurarValidacion);

        // El manjeo de errores buscar ir del mas especifico al mas general
        // Cuando salta una excepción, .NET prueba los handlers en este orden hasta que uno la agarra.
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Esto es para que Swagger pueda leer los endpoints y armar la página de prueba
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Con todo configurado, armamos la app
        var app = builder.Build();

        // Activa los handlers de errores que registramos arriba
        app.UseExceptionHandler();

        // Swagger solo se muestra cuando estamos desarrollando, no en producción
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Conecta las rutas (/api/cart/...) con los métodos de los Controllers
        app.MapControllers();

        // Prende la API y se queda escuchando pedidos
        app.Run();
    }


    // para q asp.net no corte con su 400 y el error lo manejemos nosotros
    private static void ConfigurarValidacion(ApiBehaviorOptions options)
    {
        options.SuppressModelStateInvalidFilter = true;
    }
}