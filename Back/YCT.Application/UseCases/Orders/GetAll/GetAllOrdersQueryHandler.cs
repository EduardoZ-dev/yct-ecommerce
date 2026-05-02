using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.GetAll;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, ResponseBase<List<OrderDto>>>
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<Distributor> _distributorRepository;

    public GetAllOrdersQueryHandler(
        IGenericRepository<Order> repository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<Distributor> distributorRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _distributorRepository = distributorRepository;
    }

    public async Task<ResponseBase<List<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = (await _repository.FindAsync(o => true, o => o.User, o => o.OrderDetails))
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        var productIds = orders.SelectMany(o => o.OrderDetails.Select(d => d.ProductId)).Distinct().ToList();
        var products = (await _productRepository.FindAsync(p => productIds.Contains(p.Id)))
            .ToDictionary(p => p.Id, p => p.Name);

        var distributorIds = orders.Where(o => o.DistributorId.HasValue).Select(o => o.DistributorId!.Value).Distinct().ToList();
        var distributors = distributorIds.Count > 0
            ? (await _distributorRepository.FindAsync(d => distributorIds.Contains(d.Id))).ToDictionary(d => d.Id, d => d)
            : new Dictionary<int, Distributor>();

        var dtos = orders.Select(o =>
        {
            Distributor? dist = null;
            if (o.DistributorId.HasValue) distributors.TryGetValue(o.DistributorId.Value, out dist);

            return new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Consecutive = o.Consecutive,
                OrderDate = o.OrderDate,
                Total = o.Total,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                PaidAt = o.PaidAt,
                Notes = o.Notes,
                ShippingAddress = o.ShippingAddress,
                ShippingCity = o.ShippingCity,
                ShippingLat = o.ShippingLat,
                ShippingLng = o.ShippingLng,
                ValidatedAt = o.ValidatedAt,
                ShippedAt = o.ShippedAt,
                DeliveredAt = o.DeliveredAt,
                DistributorId = o.DistributorId,
                DistributorName = dist?.Name,
                DistributorVehicle = dist != null
                    ? $"{dist.VehicleType}{(string.IsNullOrEmpty(dist.VehiclePlate) ? "" : " · " + dist.VehiclePlate)}"
                    : null,
                DistributorPhone = dist?.Phone,
                TrackingNumber = o.TrackingNumber,
                CustomerRating = o.CustomerRating,
                FeedbackComment = o.FeedbackComment,
                UserId = o.UserId,
                UserFullName = o.User?.FullName ?? "—",
                UserPhone = o.User?.Phone,
                Details = o.OrderDetails.Select(d => new OrderDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = products.TryGetValue(d.ProductId, out var name) ? name : "Producto",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }).ToList();

        return ResponseBase<List<OrderDto>>.Ok(dtos);
    }
}
