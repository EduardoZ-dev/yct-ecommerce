using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.Granjeros.Delete;

public record DeleteGranjeroCommand(int Id) : IRequest<ResponseBase<bool>>;
