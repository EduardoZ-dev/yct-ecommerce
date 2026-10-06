using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.PromedioCompra;

/// <summary>
/// A cuánto se le ha venido comprando la leche a los clientes terceros. Sirve de referencia
/// al registrar una entrega nueva, para saber si el precio que se está poniendo es el de siempre.
/// </summary>
/// <param name="Dias">Ventana hacia atrás; 0 o menos = todo el histórico.</param>
public record GetPromedioCompraTercerosQuery(int Dias = 0)
    : IRequest<ResponseBase<PromedioCompraTerceroDto>>;
