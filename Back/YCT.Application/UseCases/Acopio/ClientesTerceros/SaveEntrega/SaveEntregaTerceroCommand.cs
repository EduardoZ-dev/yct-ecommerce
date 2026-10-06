using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.SaveEntrega;

/// <param name="Id">Null o 0 registra una entrega nueva; con valor edita la existente (cantidades, fecha, precio, observación).</param>
/// <param name="PrecioLitro">Si viene null se toma el precio acordado con el cliente.</param>
/// <param name="ClientUuid">UUID generado en el cliente; si ya existe una entrega con él, se devuelve esa sin duplicar.</param>
public record SaveEntregaTerceroCommand(
    int? Id,
    int ClienteTerceroId,
    DateTime Fecha,
    int Cantinas,
    decimal SaldoLitros,
    decimal? PrecioLitro,
    string? Observacion,
    Guid? ClientUuid) : IRequest<ResponseBase<EntregaTerceroDto>>;
