using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Recepcion.GetEntregasTerceros;

/// <summary>
/// Entregas de terceros para la tablet de planta: todas las de HOY y las de los últimos días que
/// siguen sin confirmar (<c>EntregaTercero.DiasParaConfirmarEnPlanta</c>). A CIEGAS: sin lo registrado.
/// </summary>
public record GetEntregasTercerosRecepcionQuery : IRequest<ResponseBase<List<RecepcionTerceroDto>>>;
