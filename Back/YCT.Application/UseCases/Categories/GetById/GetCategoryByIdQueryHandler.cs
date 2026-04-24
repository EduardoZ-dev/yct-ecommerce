using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.GetById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ResponseBase<CategoryDto>>
{
    private readonly IGenericRepository<Category> _repository;

    public GetCategoryByIdQueryHandler(IGenericRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null)
            return ResponseBase<CategoryDto>.Fail("Categoría no encontrada");

        return ResponseBase<CategoryDto>.Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        });
    }
}
