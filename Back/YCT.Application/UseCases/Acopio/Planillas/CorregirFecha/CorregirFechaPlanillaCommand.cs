using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.CorregirFecha;

/// <summary>
/// Aplica la fecha real (deducida de las recogidas) a una planilla que llegó con la fecha
/// equivocada. Corrige SOLO la fecha; el código de la ruta se deja tal cual llegó. La oficina
/// dispara esto a mano desde el detalle, tras ver el aviso "se realizó el X pero llegó como Y".
/// </summary>
public class CorregirFechaPlanillaCommand : IRequest<ResponseBase<PlanillaDto>>
{
    public int Id { get; set; }
}
