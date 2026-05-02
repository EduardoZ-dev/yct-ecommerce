using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.Create;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ResponseBase<CategoryDto>>
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public CreateCategoryCommandHandler(IGenericRepository<Category> repository, IUnitOfWork unitOfWork, IAuditLogger audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _repository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Create", "Category", category.Id,
            $"Categoría creada: {category.Name}",
            new { category.Name, category.Description },
            ct: cancellationToken);

        return ResponseBase<CategoryDto>.Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        }, "Categoría creada exitosamente");
    }
}
