using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.Update;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ResponseBase<CategoryDto>>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public UpdateCategoryCommandHandler(IGenericRepository<Category> repository, IUnitOfWork unitOfWork, IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null)
            return ResponseBase<CategoryDto>.Fail("Categoría no encontrada");

        var before = new { category.Name, category.Description, category.IsActive };

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Update", "Category", category.Id,
            $"Categoría actualizada: {category.Name}",
            new { before, after = new { category.Name, category.Description, category.IsActive } },
            ct: cancellationToken);

        return ResponseBase<CategoryDto>.Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        }, "Categoría actualizada exitosamente");
    }
}
