using System.Security.Cryptography;
using System.Text;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Orders.CreateGuest;

public class CreateGuestOrderCommandHandler : IRequestHandler<CreateGuestOrderCommand, ResponseBase<OrderDto>>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public CreateGuestOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<ResponseBase<OrderDto>> Handle(CreateGuestOrderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return ResponseBase<OrderDto>.Fail("El nombre es obligatorio");
        if (string.IsNullOrWhiteSpace(request.Phone))
            return ResponseBase<OrderDto>.Fail("El teléfono es obligatorio");
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            return ResponseBase<OrderDto>.Fail("La dirección es obligatoria");
        if (request.Items.Count == 0)
            return ResponseBase<OrderDto>.Fail("El pedido debe tener al menos un producto");

        // Buscar o crear cliente por teléfono
        var phone = NormalizePhone(request.Phone);
        var existing = await _userRepository.FindAsync(u => u.Phone == phone && u.Role == Roles.Customer);
        var customer = existing.FirstOrDefault();

        if (customer == null)
        {
            // Crea un Customer "guest" con el teléfono como username único
            var username = $"guest_{phone}";
            var randomPwd = Guid.NewGuid().ToString("N");
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(randomPwd))).ToLower();

            customer = new User
            {
                Username = username,
                PasswordHash = hash,
                FullName = request.FullName.Trim(),
                Phone = phone,
                Role = Roles.Customer,
                IsActive = true
            };
            await _userRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (customer.FullName != request.FullName.Trim())
        {
            // Actualiza el nombre si el cliente ya existía y dio uno distinto
            customer.FullName = request.FullName.Trim();
            customer.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(customer);
        }

        // Calcular siguiente consecutivo
        var allOrders = await _orderRepository.GetAllAsync();
        var nextConsecutive = allOrders.Any() ? allOrders.Max(o => o.Consecutive) + 1 : 1;

        // Validar método de pago
        var validMethods = new[] { "OnDelivery", "Transfer", "Cash" };
        var paymentMethod = validMethods.Contains(request.PaymentMethod) ? request.PaymentMethod : "OnDelivery";

        // Construir orden
        var order = new Order
        {
            OrderNumber = $"YCT-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Consecutive = nextConsecutive,
            UserId = customer.Id,
            Notes = request.Notes?.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            ShippingCity = request.ShippingCity?.Trim(),
            ShippingLat = request.ShippingLat,
            ShippingLng = request.ShippingLng,
            Status = "Pending",
            PaymentMethod = paymentMethod,
            PaymentStatus = "Unpaid"
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                return ResponseBase<OrderDto>.Fail($"Producto con ID {item.ProductId} no encontrado");

            if (!product.IsActive)
                return ResponseBase<OrderDto>.Fail($"El producto '{product.Name}' ya no está disponible");

            if (product.Stock < item.Quantity)
                return ResponseBase<OrderDto>.Fail($"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}");

            var subtotal = product.Price * item.Quantity;
            total += subtotal;

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Subtotal = subtotal
            });

            product.Stock -= item.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        order.Total = total;
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("Create", "Order", order.Id,
            $"Pedido recibido: {order.OrderNumber} de {customer.FullName} ({phone})",
            new { order.OrderNumber, customer = customer.FullName, phone, total = order.Total, items = request.Items.Count },
            overrideUserId: customer.Id,
            overrideUsername: customer.Username,
            ct: cancellationToken);

        return ResponseBase<OrderDto>.Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Consecutive = order.Consecutive,
            OrderDate = order.OrderDate,
            Total = order.Total,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            Notes = order.Notes,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingLat = order.ShippingLat,
            ShippingLng = order.ShippingLng,
            UserId = order.UserId,
            UserFullName = customer.FullName,
            UserPhone = customer.Phone,
            Details = order.OrderDetails.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        }, "Pedido recibido exitosamente");
    }

    private static string NormalizePhone(string phone)
    {
        // Mantiene solo dígitos y el +
        return new string(phone.Trim().Where(c => char.IsDigit(c) || c == '+').ToArray());
    }
}
