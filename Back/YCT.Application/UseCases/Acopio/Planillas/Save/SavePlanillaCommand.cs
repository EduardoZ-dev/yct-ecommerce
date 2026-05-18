using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.Save;

public record SavePlanillaCommand(
    int? Id,
    string Codigo,
    DateTime Fecha,
    int CamionId,
    int ConductorId,
    int? AsistenteId,
    TimeSpan? HoraSalida,
    TimeSpan? HoraLlegadaPlanta,
    TimeSpan? HoraDescargue,
    string? Observaciones,
    List<SavePlanillaItemRequest> Items
) : IRequest<ResponseBase<PlanillaDto>>;
