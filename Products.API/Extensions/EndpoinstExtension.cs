using Products.API.Extensions.Endpoints;

namespace Products.API.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan
        app.MapProductoEndpoints();
    }
}
