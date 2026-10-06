using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Acopio.ClientesTerceros.DeleteEntrega;

public record DeleteEntregaTerceroCommand(int Id) : IRequest<ResponseBase<bool>>;
