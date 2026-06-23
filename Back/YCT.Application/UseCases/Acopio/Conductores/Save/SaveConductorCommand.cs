using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Conductores.Save;

public record SaveConductorCommand(
    int? Id,
    string NombreCompleto,
    string? Cedula,
    string? Telefono,
    int? CamionPreferidoId,
    int? UserId,
    bool IsActive,
    /// <summary>PIN en texto plano para login en la app móvil. Null = no cambiar. Vacío = quitar acceso.</summary>
    string? Pin = null) : IRequest<ResponseBase<ConductorDto>>;
