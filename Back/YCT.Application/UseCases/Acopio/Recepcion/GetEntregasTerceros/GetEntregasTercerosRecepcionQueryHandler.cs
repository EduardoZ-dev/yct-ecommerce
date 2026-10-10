using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.Recepcion.GetEntregasTerceros;

public class GetEntregasTercerosRecepcionQueryHandler
    : IRequestHandler<GetEntregasTercerosRecepcionQuery, ResponseBase<List<RecepcionTerceroDto>>>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly TimeProvider _reloj;

    public GetEntregasTercerosRecepcionQueryHandler(IGenericRepository<EntregaTercero> entregaRepository, TimeProvider reloj)
    {
        _entregaRepository = entregaRepository;
        _reloj = reloj;
    }

    public async Task<ResponseBase<List<RecepcionTerceroDto>>> Handle(
        GetEntregasTercerosRecepcionQuery request, CancellationToken cancellationToken)
    {
        var hoy = ColombiaTime.FromUtc(_reloj.GetUtcNow().UtcDateTime).Date;
        var manana = hoy.AddDays(1);
        var desde = hoy.AddDays(-EntregaTercero.DiasParaConfirmarEnPlanta);

        // Se incluye el cliente en la misma consulta para no pedir su nombre entrega por entrega.
        var entregas = await _entregaRepository.FindAsync(
            e => e.Fecha < manana
                 && (e.Fecha >= hoy || (e.Fecha >= desde && e.ConfirmadaEnPlantaAt == null)),
            e => e.ClienteTercero);

        var dtos = entregas
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.CreatedAt)
            .Select(ToDto)
            .ToList();
        return ResponseBase<List<RecepcionTerceroDto>>.Ok(dtos);
    }

    /// <summary>A ciegas: ni lo registrado (cantinas, litros), ni su nota, ni precio; solo lo que midió planta.</summary>
    private static RecepcionTerceroDto ToDto(EntregaTercero e) => new()
    {
        Id = e.Id,
        Fecha = e.Fecha,
        ClienteNombre = e.ClienteTercero.NombreCompleto,
        Municipio = e.ClienteTercero.Municipio,
        Confirmada = e.ConfirmadaEnPlantaAt.HasValue,
        ConfirmadaEnPlantaAt = e.ConfirmadaEnPlantaAt,
        CantinasPlanta = e.CantinasPlanta,
        SaldoPlanta = e.SaldoPlanta,
        LitrosPlanta = e.LitrosPlanta,
        RegistradoPorNombre = e.RegistradoPorNombre,
        CreatedAt = e.CreatedAt
    };
}
