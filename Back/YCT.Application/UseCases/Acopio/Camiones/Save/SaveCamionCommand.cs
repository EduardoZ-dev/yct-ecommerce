using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Camiones.Save;

public record SaveCamionCommand(
    int? Id,
    string Nombre,
    string? Placa,
    string Estado,
    string? Notas) : IRequest<ResponseBase<CamionDto>>;
