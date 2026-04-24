using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Categories.GetAll;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, ResponseBase<List<CategoryDto>>>
{
    private readonly IGenericRepository<Category> _repository;

    public GetAllCategoriesQueryHandler(IGenericRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ProductCount = c.Products?.Count ?? 0
        }).ToList();

        return ResponseBase<List<CategoryDto>>.Ok(dtos);
    }
}
