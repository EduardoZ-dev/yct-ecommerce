using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.ConfirmarEntrega;

/// <summary>
/// Reafirma que la leche de una entrega de tercero llegó a planta. El panel la REGISTRA;
/// esto solo marca la llegada, así que es idempotente: confirmar dos veces no cambia nada.
/// </summary>
public record ConfirmarEntregaTerceroCommand(int Id) : IRequest<ResponseBase<EntregaTerceroDto>>;
