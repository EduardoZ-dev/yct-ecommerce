using MediatR;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

/// <summary>
/// Una entrega de tercero se confirmó en planta CON medición. El caso de uso solo lo anuncia;
/// quien quiera reaccionar (el WhatsApp) lo escucha aparte, en segundo plano.
/// </summary>
public record EntregaTerceroConfirmada(int EntregaId) : INotification;
