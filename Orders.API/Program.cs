using Orders.API.Clients;
using Orders.API.Data;
using Orders.API.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la bd
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<OrdenRepository>();

// Handlers de errores: primero los especificos y al final el general
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Clientes para llamar a Users.API y Products.API
builder.Services.AddHttpClient<UsersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UsersApi"] ?? "http://localhost:5102");
});
builder.Services.AddHttpClient<ProductsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductsApi"] ?? "http://localhost:5101");
});

var app = builder.Build();

// Inicializar bd
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

// Activa los handlers de errores
app.UseExceptionHandler();

app.MapGet("/", () => "Orders.API funcionando");

app.Run();