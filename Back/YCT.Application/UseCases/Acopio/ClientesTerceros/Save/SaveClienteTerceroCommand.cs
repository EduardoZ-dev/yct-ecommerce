using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.Save;

public record SaveClienteTerceroCommand(
    int? Id,
    string NombreCompleto,
    string? Cedula,
    string? Telefono,
    string? Municipio,
    decimal? PrecioLitro,
    string? Notas,
    bool IsActive) : IRequest<ResponseBase<ClienteTerceroDto>>;
