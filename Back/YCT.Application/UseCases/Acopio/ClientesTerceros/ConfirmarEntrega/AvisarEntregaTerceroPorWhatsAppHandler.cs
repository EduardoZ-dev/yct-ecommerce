using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities.Acopio;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

/// <summary>
/// Manda por WhatsApp lo registrado vs lo recibido cuando planta confirma una entrega de tercero.
/// Corre en segundo plano (ver <see cref="ConfirmarEntregaTerceroCommandHandler"/>): si Meta tarda
/// o falla, la confirmación ya quedó guardada y la tablet no esperó.
/// </summary>
public class AvisarEntregaTerceroPorWhatsAppHandler : INotificationHandler<EntregaTerceroConfirmada>
{
    private readonly IGenericRepository<EntregaTercero> _entregaRepository;
    private readonly IWhatsAppNotifier _whatsApp;

    public AvisarEntregaTerceroPorWhatsAppHandler(
        IGenericRepository<EntregaTercero> entregaRepository, IWhatsAppNotifier whatsApp)
    {
        _entregaRepository = entregaRepository;
        _whatsApp = whatsApp;
    }

    public async Task Handle(EntregaTerceroConfirmada notification, CancellationToken cancellationToken)
    {
        var coincidencias = await _entregaRepository.FindAsync(e => e.Id == notification.EntregaId, e => e.ClienteTercero);
        var entrega = coincidencias.FirstOrDefault();
        // Sin medición no hay variación que reportar (o la entrega se borró antes del aviso).
        if (entrega?.LitrosPlanta is not decimal recibido)
            return;

        var diferencia = recibido - entrega.Litros;
        await _whatsApp.SendTerceroAsync(new WhatsAppTerceroModel(
            Resultado: Resultado(diferencia),
            Cliente: entrega.ClienteTercero.NombreCompleto,
            Fecha: entrega.Fecha,
            LitrosRegistrados: entrega.Litros,
            LitrosRecibidos: recibido,
            Diferencia: diferencia,
            Observacion: entrega.ObservacionPlanta ?? string.Empty,
            RecibidoPor: entrega.ConfirmadaPorNombre ?? "Planta"), cancellationToken);
    }

    private static string Resultado(decimal diferencia) => diferencia switch
    {
        0 => "✅ Llegó completa",
        < 0 => "🚨 Llegó MENOS leche",
        _ => "⬆️ Llegó MÁS leche",
    };
}
