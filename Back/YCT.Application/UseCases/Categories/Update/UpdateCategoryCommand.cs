using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Categories.Update;

public record UpdateCategoryCommand(int Id, string Name, string? Description, bool IsActive) : IRequest<ResponseBase<CategoryDto>>;
