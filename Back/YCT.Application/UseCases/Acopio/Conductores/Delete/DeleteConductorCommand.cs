using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Conductores.Delete;

public record DeleteConductorCommand(int Id) : IRequest<ResponseBase<bool>>;
