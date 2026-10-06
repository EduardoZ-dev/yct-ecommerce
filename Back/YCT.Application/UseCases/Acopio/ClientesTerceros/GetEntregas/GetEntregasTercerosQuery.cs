using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.GetEntregas;

/// <param name="Dias">Ventana hacia atrás desde hoy; con 0 o negativo se usa <see cref="DiasPorDefecto"/>.</param>
/// <param name="ClienteTerceroId">Filtra por cliente; null trae las de todos.</param>
/// <param name="Fecha">
/// Un solo día. Lo usa la validación del descargue para ver qué leche de terceros entró
/// esa misma fecha. Cuando viene, manda sobre <paramref name="Dias"/>.
/// </param>
public record GetEntregasTercerosQuery(int Dias, int? ClienteTerceroId, DateTime? Fecha = null)
    : IRequest<ResponseBase<List<EntregaTerceroDto>>>
{
    public const int DiasPorDefecto = 30;
}
