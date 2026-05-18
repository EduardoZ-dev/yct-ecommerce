using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.GetById;

public record GetPlanillaByIdQuery(int Id) : IRequest<ResponseBase<PlanillaDto>>;
