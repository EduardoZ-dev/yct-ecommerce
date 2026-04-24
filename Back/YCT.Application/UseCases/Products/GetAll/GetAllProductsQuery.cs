using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.GetAll;

public record GetAllProductsQuery : IRequest<ResponseBase<List<ProductDto>>>;
