using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Categories.GetAll;

public record GetAllCategoriesQuery : IRequest<ResponseBase<List<CategoryDto>>>;
