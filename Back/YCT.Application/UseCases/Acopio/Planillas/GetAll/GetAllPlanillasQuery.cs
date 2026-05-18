using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Acopio.Planillas.GetAll;

public record GetAllPlanillasQuery() : IRequest<ResponseBase<List<PlanillaHeaderDto>>>;
