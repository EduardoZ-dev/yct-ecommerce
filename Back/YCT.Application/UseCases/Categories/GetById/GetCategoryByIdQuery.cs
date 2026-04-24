using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Categories.GetById;

public record GetCategoryByIdQuery(int Id) : IRequest<ResponseBase<CategoryDto>>;
