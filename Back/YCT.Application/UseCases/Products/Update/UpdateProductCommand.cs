using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.Update;

public record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    int CategoryId) : IRequest<ResponseBase<ProductDto>>;
