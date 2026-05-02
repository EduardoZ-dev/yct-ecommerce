using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.Reorder;

public class ReorderProductsCommandHandler : IRequestHandler<ReorderProductsCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public ReorderProductsCommandHandler(
        IGenericRepository<Product> repository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(ReorderProductsCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            return ResponseBase<bool>.Ok(true);

        var ids = request.Items.Select(i => i.Id).ToList();
        var products = (await _repository.FindAsync(p => ids.Contains(p.Id))).ToList();

        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.Id);
            if (product != null)
            {
                product.DisplayOrder = item.DisplayOrder;
                product.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(product);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Reorder", "Product", null,
            $"Productos reordenados ({request.Items.Count})",
            new { count = request.Items.Count, ids },
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Orden actualizado");
    }
}
