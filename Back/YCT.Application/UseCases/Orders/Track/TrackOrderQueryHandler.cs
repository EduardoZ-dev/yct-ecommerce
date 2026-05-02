using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.Track;

public class TrackOrderQueryHandler : IRequestHandler<TrackOrderQuery, ResponseBase<List<TrackedOrderDto>>>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<Distributor> _distributorRepository;

    public TrackOrderQueryHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<User> userRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<Distributor> distributorRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _distributorRepository = distributorRepository;
    }

    public async Task<ResponseBase<List<TrackedOrderDto>>> Handle(TrackOrderQuery request, CancellationToken cancellationToken)
    {
        var raw = request.Search?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            return ResponseBase<List<TrackedOrderDto>>.Ok(new List<TrackedOrderDto>());

        // Limpieza: quitar #, espacios extra. Mantenemos guiones por si es OrderNumber.
        var search = raw.TrimStart('#').Trim();

        var orders = new List<Order>();

        // 1) Buscar por OrderNumber (case-insensitive). Cualquier string con guión o que empiece con YCT.
        if (search.Contains('-') || search.StartsWith("YCT", StringComparison.OrdinalIgnoreCase))
        {
            var upperSearch = search.ToUpperInvariant();
            orders = (await _orderRepository.FindAsync(o => o.OrderNumber.ToUpper() == upperSearch, o => o.User, o => o.OrderDetails)).ToList();
        }

        // 2) Si no se encontró y es un número entero, buscar por Consecutive
        if (!orders.Any() && int.TryParse(search, out var consecutive))
        {
            orders = (await _orderRepository.FindAsync(o => o.Consecutive == consecutive, o => o.User, o => o.OrderDetails)).ToList();
        }

        // 3) Si sigue vacío y parece teléfono, buscar por usuario
        if (!orders.Any())
        {
            var phone = new string(search.Where(c => char.IsDigit(c) || c == '+').ToArray());
            if (phone.Length >= 6)
            {
                var users = await _userRepository.FindAsync(u => u.Phone == phone);
                var userIds = users.Select(u => u.Id).ToList();
                if (userIds.Count > 0)
                {
                    orders = (await _orderRepository.FindAsync(o => userIds.Contains(o.UserId), o => o.User, o => o.OrderDetails))
                        .OrderByDescending(o => o.OrderDate)
                        .Take(20)
                        .ToList();
                }
            }
        }

        if (!orders.Any())
            return ResponseBase<List<TrackedOrderDto>>.Ok(new List<TrackedOrderDto>(), "No se encontraron pedidos");

        var productIds = orders.SelectMany(o => o.OrderDetails.Select(d => d.ProductId)).Distinct().ToList();
        var products = (await _productRepository.FindAsync(p => productIds.Contains(p.Id))).ToDictionary(p => p.Id, p => p.Name);

        var distIds = orders.Where(o => o.DistributorId.HasValue).Select(o => o.DistributorId!.Value).Distinct().ToList();
        var distributors = distIds.Count > 0
            ? (await _distributorRepository.FindAsync(d => distIds.Contains(d.Id))).ToDictionary(d => d.Id, d => d)
            : new Dictionary<int, Distributor>();

        var dtos = orders.Select(o =>
        {
            Distributor? dist = null;
            if (o.DistributorId.HasValue) distributors.TryGetValue(o.DistributorId.Value, out dist);

            return new TrackedOrderDto
            {
                OrderNumber = o.OrderNumber,
                Consecutive = o.Consecutive,
                OrderDate = o.OrderDate,
                Total = o.Total,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                ValidatedAt = o.ValidatedAt,
                ShippedAt = o.ShippedAt,
                DeliveredAt = o.DeliveredAt,
                DistributorName = dist?.Name,
                DistributorVehicle = dist != null
                    ? $"{dist.VehicleType}{(string.IsNullOrEmpty(dist.VehiclePlate) ? "" : " · " + dist.VehiclePlate)}"
                    : null,
                TrackingNumber = o.TrackingNumber,
                CustomerName = o.User?.FullName ?? string.Empty,
                ShippingAddress = o.ShippingAddress ?? string.Empty,
                ShippingCity = o.ShippingCity,
                ShippingLat = o.ShippingLat,
                ShippingLng = o.ShippingLng,
                Items = o.OrderDetails.Select(d => new OrderDetailDto
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

        return ResponseBase<List<TrackedOrderDto>>.Ok(dtos);
    }
}
