using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.Delete;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IGenericRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null)
            return ResponseBase<bool>.Fail("Categoría no encontrada");

        await _repository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResponseBase<bool>.Ok(true, "Categoría eliminada exitosamente");
    }
}
