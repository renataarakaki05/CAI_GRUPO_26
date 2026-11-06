namespace Notifications.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // Registra las rutas de la carpeta Controllers
        app.MapControllers();
    }
}
