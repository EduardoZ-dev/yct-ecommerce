using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Tinas.UpdatePlanta;

/// <summary>
/// Ajusta las tinas de YCT en la planta y/o agrega una observación (ej. "tina dañada").
/// Si la cantidad no cambia pero hay observación, queda como nota en el historial.
/// </summary>
public record UpdateTinaPlantaCommand(int Cantidad, string? Observacion) : IRequest<ResponseBase<bool>>;
