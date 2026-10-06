using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.GetEntregas;

public class GetEntregasTercerosQueryHandler : IRequestHandler<GetEntregasTercerosQuery, ResponseBase<List<EntregaTerceroDto>>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IGenericRepository<Ruta> _rutaRepository;

    public GetEntregasTercerosQueryHandler(
        IGenericRepository<EntregaTercero> entregaRepository,
        IGenericRepository<Ruta> rutaRepository)
    {
        _entregaRepository = entregaRepository;
        _rutaRepository = rutaRepository;
    }

    public async Task<ResponseBase<List<EntregaTerceroDto>>> Handle(GetEntregasTercerosQuery request, CancellationToken cancellationToken)
    {
        var dias = request.Dias > 0 ? request.Dias : GetEntregasTercerosQuery.DiasPorDefecto;
        var desde = DateTime.UtcNow.Date.AddDays(-dias);
        var clienteId = request.ClienteTerceroId;
        // Un día concreto (lo pide la validación del descargue) manda sobre la ventana de días.
        var dia = request.Fecha?.Date;

        var entregas = await _entregaRepository.FindAsync(
            e => (dia == null ? e.Fecha >= desde : e.Fecha == dia)
                 && (clienteId == null || e.ClienteTerceroId == clienteId),
            e => e.ClienteTercero);

        var codigosRuta = await ObtenerCodigosRutaAsync(entregas);

        var dtos = entregas
            .OrderByDescending(e => e.Fecha)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => ClientesTercerosMapper.ToDto(
                e,
                e.ClienteTercero.NombreCompleto,
                e.RutaId.HasValue ? codigosRuta.GetValueOrDefault(e.RutaId.Value) : null))
            .ToList();

        return ResponseBase<List<EntregaTerceroDto>>.Ok(dtos);
    }

    /// <summary>
    /// La entidad no navega a Ruta (el vínculo es solo informativo), así que los códigos
    /// se traen en una única consulta por IN en vez de una por entrega.
    /// </summary>
    private async Task<Dictionary<int, string>> ObtenerCodigosRutaAsync(IReadOnlyList<EntregaTercero> entregas)
    {
        var rutaIds = entregas
            .Where(e => e.RutaId.HasValue)
            .Select(e => e.RutaId!.Value)
            .Distinct()
            .ToList();
        if (rutaIds.Count == 0)
            return new Dictionary<int, string>();

        var rutas = await _rutaRepository.FindAsync(r => rutaIds.Contains(r.Id));
        return rutas.ToDictionary(r => r.Id, r => r.Codigo);
    }
}
