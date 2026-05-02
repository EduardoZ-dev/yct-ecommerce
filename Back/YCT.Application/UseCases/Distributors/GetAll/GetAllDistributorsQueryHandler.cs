using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Distributors.GetAll;

public class GetAllDistributorsQueryHandler : IRequestHandler<GetAllDistributorsQuery, ResponseBase<List<DistributorDto>>>
{
    private readonly IGenericRepository<Distributor> _repository;
    private readonly IGenericRepository<Order> _orderRepository;

    public GetAllDistributorsQueryHandler(
        IGenericRepository<Distributor> repository,
        IGenericRepository<Order> orderRepository)
    {
        _repository = repository;
        _orderRepository = orderRepository;
    }

    public async Task<ResponseBase<List<DistributorDto>>> Handle(GetAllDistributorsQuery request, CancellationToken cancellationToken)
    {
        var all = (await _repository.GetAllAsync()).ToList();
        var orders = await _orderRepository.GetAllAsync();

        var counts = orders
            .Where(o => o.DistributorId.HasValue)
            .GroupBy(o => o.DistributorId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = all
            .OrderByDescending(d => d.IsActive)
            .ThenBy(d => d.Name)
            .Select(d => new DistributorDto
            {
                Id = d.Id,
                Name = d.Name,
                Phone = d.Phone,
                VehicleType = d.VehicleType,
                VehiclePlate = d.VehiclePlate,
                Notes = d.Notes,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                OrdersCount = counts.GetValueOrDefault(d.Id, 0)
            }).ToList();

        return ResponseBase<List<DistributorDto>>.Ok(dtos);
    }
}
