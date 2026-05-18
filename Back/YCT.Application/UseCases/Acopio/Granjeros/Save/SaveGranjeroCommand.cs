using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Granjeros.Save;

public record SaveGranjeroCommand(
    int? Id,
    int Numero,
    string NombreCompleto,
    string? Cedula,
    string? Telefono,
    string? Finca,
    string? Vereda,
    string? Municipio,
    decimal? PrecioLitro,
    string? Notas,
    bool IsActive) : IRequest<ResponseBase<GranjeroDto>>;
