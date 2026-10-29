using Notifications.API.Extensions.Endpoints;

namespace Notifications.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan
        app.MapNotificacionEndpoints();
    }
}
