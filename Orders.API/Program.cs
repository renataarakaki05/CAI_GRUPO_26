using Orders.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la bd
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<OrdenRepository>();

var app = builder.Build();

// Inicializar bd
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

app.MapGet("/", () => "Orders.API funcionando");

app.Run();