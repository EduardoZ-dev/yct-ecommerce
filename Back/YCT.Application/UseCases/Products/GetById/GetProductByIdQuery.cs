using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.GetById;

public record GetProductByIdQuery(int Id) : IRequest<ResponseBase<ProductDto>>;
