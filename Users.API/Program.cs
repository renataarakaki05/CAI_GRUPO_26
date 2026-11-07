using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.Extensions;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la bd
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<UsuarioRepository>();

// Servicio de usuarios (registro, login y buscar por id)
builder.Services.AddScoped<UsuarioService>();

// Handlers de errores: primero los especificos y al final el general
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Si el JSON viene mal escrito, tira error (así lo agarra ValidationExceptionHandler)
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

// Swagger (igual que en MiniApi)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar bd
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

// Activa los handlers de errores
app.UseExceptionHandler();

// Swagger solo en Development (igual que en MiniApi)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints de usuarios
app.MapAppEndpoints();

app.Run();