using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Asistentes.Save;

public record SaveAsistenteCommand(
    int? Id,
    string NombreCompleto,
    string? Cedula,
    string? Telefono,
    int? CamionPreferidoId,
    bool IsActive) : IRequest<ResponseBase<AsistenteDto>>;
