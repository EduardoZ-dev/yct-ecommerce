using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Tinas.UpdateFinca;

/// <summary>Ajusta las tinas de YCT en una finca (código) y deja el movimiento en el historial.</summary>
public record UpdateTinaFincaCommand(int CodigoId, int Cantidad, string? Observacion) : IRequest<ResponseBase<bool>>;
