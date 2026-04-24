using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.Create;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    int CategoryId) : IRequest<ResponseBase<ProductDto>>;
