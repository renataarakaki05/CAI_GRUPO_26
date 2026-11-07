using System.Runtime.InteropServices;

public partial class Program
{
    // Acá arranca la API, es lo primero que se ejecuta
    private static void Main(string[] args)
    {
        // El builder es como la "caja de herramientas": acá le decimos qué cosas va a usar la API
        var builder = WebApplication.CreateBuilder(args);

        // Le avisamos que vamos a usar Controllers (las clases donde van los endpoints)
        builder.Services.AddControllers();

        // Esto es para que Swagger pueda leer los endpoints y armar la página de prueba
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Con todo configurado, armamos la app
        var app = builder.Build();

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
}
