using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Dashboard.GetMetrics;

public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, ResponseBase<DashboardMetricsDto>>
{
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly IGenericRepository<Order> _orderRepo;

    public GetMetricsQueryHandler(
        IGenericRepository<Product> productRepo,
        IGenericRepository<Category> categoryRepo,
        IGenericRepository<Order> orderRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _orderRepo = orderRepo;
    }

    public async Task<ResponseBase<DashboardMetricsDto>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
    {
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // Conteos agregados directos en SQL
        var totalProducts = await _productRepo.CountAsync();
        var activeProducts = await _productRepo.CountAsync(p => p.IsActive);
        var lowStockProducts = await _productRepo.CountAsync(p => p.IsActive && p.Stock > 0 && p.Stock <= 20);
        var totalCategories = await _categoryRepo.CountAsync(c => c.IsActive);
        var totalOrders = await _orderRepo.CountAsync();
        var pendingOrders = await _orderRepo.CountAsync(o => o.Status == "Pending");

        // Sumas agregadas en SQL
        var monthlyRevenue = await _orderRepo.SumDecimalAsync(
            o => o.OrderDate >= firstOfMonth && o.Status != "Cancelled",
            o => o.Total);

        var totalRevenue = await _orderRepo.SumDecimalAsync(
            o => o.Status != "Cancelled",
            o => o.Total);

        // Listas pequeñas (top 5)
        var recentOrders = (await _orderRepo.FindAsync(o => true, o => o.User))
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new RecentOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Total = o.Total,
                Status = o.Status,
                CustomerName = o.User?.FullName ?? "—"
            })
            .ToList();

        var lowStockList = (await _productRepo.FindAsync(p => p.IsActive && p.Stock > 0 && p.Stock <= 20))
            .OrderBy(p => p.Stock)
            .Take(5)
            .Select(p => new TopProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            })
            .ToList();

        var dto = new DashboardMetricsDto
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            LowStockProducts = lowStockProducts,
            TotalCategories = totalCategories,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            MonthlyRevenue = monthlyRevenue,
            TotalRevenue = totalRevenue,
            RecentOrders = recentOrders,
            LowStockList = lowStockList
        };

        return ResponseBase<DashboardMetricsDto>.Ok(dto);
    }
}
