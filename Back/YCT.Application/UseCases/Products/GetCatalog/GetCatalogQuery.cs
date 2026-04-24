using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Products.GetCatalog;

public record GetCatalogQuery : IRequest<ResponseBase<List<ProductDto>>>;
