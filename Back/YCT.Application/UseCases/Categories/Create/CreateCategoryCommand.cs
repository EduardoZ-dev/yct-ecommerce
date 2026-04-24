using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Categories.Create;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<ResponseBase<CategoryDto>>;
