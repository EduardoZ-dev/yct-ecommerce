using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Categories.Delete;

public record DeleteCategoryCommand(int Id) : IRequest<ResponseBase<bool>>;
