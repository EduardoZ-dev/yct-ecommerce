using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Camiones.Delete;

public record DeleteCamionCommand(int Id) : IRequest<ResponseBase<bool>>;
