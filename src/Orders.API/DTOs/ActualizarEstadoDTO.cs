namespace Orders.API.DTOs;

/// <summary>Estado nuevo de la orden.</summary>
/// <param name="Estado" example="Confirmada">Confirmada, Enviada, Entregada o Cancelada.</param>
public record ActualizarEstadoDto( // Request para cambiar el estado
    string Estado
);

/// <summary>Resultado de cambiar el estado de una orden.</summary>
/// <param name="Id" example="f1e2d3c4-0000-0000-0000-aabbccddeeff">Id de la orden.</param>
/// <param name="Estado" example="Confirmada">Estado actual de la orden.</param>
/// <param name="FechaActualizacion" example="2024-03-10T12:00:00Z">Fecha del cambio de estado (UTC).</param>
public record EstadoActualizadoDto( // Response del cambio de estado
    Guid Id,
    string Estado,
    DateTime FechaActualizacion
);