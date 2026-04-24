using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Products.Delete;

public record DeleteProductCommand(int Id) : IRequest<ResponseBase<bool>>;
