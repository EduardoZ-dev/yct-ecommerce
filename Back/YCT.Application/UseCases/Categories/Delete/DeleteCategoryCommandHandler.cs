using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.Delete;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public DeleteCategoryCommandHandler(IGenericRepository<Category> repository, IUnitOfWork unitOfWork, IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null)
            return ResponseBase<bool>.Fail("Categoría no encontrada");

        var snapshot = new { category.Name, category.Description };

        await _repository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Delete", "Category", request.Id,
            $"Categoría eliminada: {snapshot.Name}",
            snapshot,
            ct: cancellationToken);

        return ResponseBase<bool>.Ok(true, "Categoría eliminada exitosamente");
    }
}
