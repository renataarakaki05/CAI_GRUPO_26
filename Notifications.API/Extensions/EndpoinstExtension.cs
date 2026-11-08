namespace Notifications.API.Extensions;

public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // registra las rutas de la carpeta Controllers
        app.MapControllers();
    }
}
