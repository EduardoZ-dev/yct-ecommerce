using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

/// <summary>
/// Reafirma que la leche de una entrega de tercero llegó a planta. El panel la REGISTRA;
/// esto marca la llegada y, desde la tablet, guarda lo que el receptor MIDIÓ. Es idempotente:
/// confirmar dos veces no cambia nada (ni quién, ni cuándo, ni la medición).
/// </summary>
/// <param name="Id">Entrega a confirmar.</param>
/// <param name="Medicion">Lo que llegó según el receptor. Null = se marca sin medir (panel).</param>
public record ConfirmarEntregaTerceroCommand(int Id, MedicionTercero? Medicion = null)
    : IRequest<ResponseBase<EntregaTerceroDto>>;

/// <summary>Medición en planta: cantinas × 40 + saldo (siempre &lt; 40), igual que en el registro.</summary>
public record MedicionTercero(int Cantinas, decimal SaldoLitros, string? Observacion);
