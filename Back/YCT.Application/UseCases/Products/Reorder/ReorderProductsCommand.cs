using MediatR;
using YCT.Application.Common;

namespace YCT.Application.UseCases.Products.Reorder;

public record ReorderProductsCommand(List<ReorderItem> Items) : IRequest<ResponseBase<bool>>;

public record ReorderItem(int Id, int DisplayOrder);
