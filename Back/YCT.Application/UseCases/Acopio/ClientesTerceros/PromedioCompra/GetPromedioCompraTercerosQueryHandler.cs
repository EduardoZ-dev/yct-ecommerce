using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.PromedioCompra;

public class GetPromedioCompraTercerosQueryHandler
    : IRequestHandler<GetPromedioCompraTercerosQuery, ResponseBase<PromedioCompraTerceroDto>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;

    public GetPromedioCompraTercerosQueryHandler(IGenericRepository<EntregaTercero> entregaRepository)
    {
        _entregaRepository = entregaRepository;
    }

    public async Task<ResponseBase<PromedioCompraTerceroDto>> Handle(
        GetPromedioCompraTercerosQuery request, CancellationToken cancellationToken)
    {
        var entregas = await LeerEntregasAsync(request.Dias);

        // Solo cuentan las que tienen precio y litros: una entrega sin precio no dice
        // nada de a cuánto se compró, y meterla bajaría el promedio artificialmente.
        var conPrecio = entregas
            .Where(e => e.PrecioLitro.HasValue && e.PrecioLitro.Value > 0 && e.Litros > 0)
            .ToList();

        if (conPrecio.Count == 0)
            return ResponseBase<PromedioCompraTerceroDto>.Ok(new PromedioCompraTerceroDto());

        var litros = conPrecio.Sum(e => e.Litros);
        var valor = conPrecio.Sum(e => e.Litros * e.PrecioLitro!.Value);

        return ResponseBase<PromedioCompraTerceroDto>.Ok(new PromedioCompraTerceroDto
        {
            // Ponderado por litros: es el precio al que de verdad se compró la leche.
            PrecioPromedio = Math.Round(valor / litros, 2),
            PrecioMinimo = conPrecio.Min(e => e.PrecioLitro!.Value),
            PrecioMaximo = conPrecio.Max(e => e.PrecioLitro!.Value),
            Entregas = conPrecio.Count,
            Litros = litros,
            ValorTotal = valor,
            Desde = conPrecio.Min(e => e.Fecha)
        });
    }

    private async Task<IReadOnlyList<EntregaTercero>> LeerEntregasAsync(int dias)
    {
        if (dias <= 0) return await _entregaRepository.GetAllAsync();   // todo el histórico
        var desde = DateTime.UtcNow.Date.AddDays(-dias);
        return await _entregaRepository.FindAsync(e => e.Fecha >= desde);
    }
}
