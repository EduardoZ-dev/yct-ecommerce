using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.Delete;

public record DeleteClienteTerceroCommand(int Id) : IRequest<ResponseBase<bool>>;
