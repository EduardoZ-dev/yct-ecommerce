using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Planillas.Delete;

public record DeletePlanillaCommand(int Id) : IRequest<ResponseBase<bool>>;
