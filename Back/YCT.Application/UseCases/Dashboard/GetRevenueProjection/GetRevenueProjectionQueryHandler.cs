using System.Globalization;
using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Dashboard.GetRevenueProjection;

public class GetRevenueProjectionQueryHandler : IRequestHandler<GetRevenueProjectionQuery, ResponseBase<RevenueProjectionDto>>
{
    private readonly IGenericRepository<Order> _orderRepo;
    private static readonly CultureInfo Es = new("es-CO");

    public GetRevenueProjectionQueryHandler(IGenericRepository<Order> orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<ResponseBase<RevenueProjectionDto>> Handle(GetRevenueProjectionQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startWindow = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

        // Carga acotada: solo pedidos no cancelados de los últimos 12 meses
        var windowOrders = await _orderRepo.FindAsync(o =>
            o.Status != "Cancelled" && o.OrderDate >= startWindow);

        // Agrupa en memoria (12 meses × N pedidos por mes — acotado)
        var grouped = windowOrders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month),
                          g => (Revenue: g.Sum(o => o.Total), Count: g.Count()));

        var months = new List<MonthlyRevenueDto>();
        for (int i = 0; i < 12; i++)
        {
            var d = startWindow.AddMonths(i);
            grouped.TryGetValue((d.Year, d.Month), out var bucket);
            var label = char.ToUpper(d.ToString("MMM yyyy", Es)[0]) + d.ToString("MMM yyyy", Es)[1..];

            months.Add(new MonthlyRevenueDto
            {
                Year = d.Year,
                Month = d.Month,
                Label = label,
                Revenue = bucket.Revenue,
                OrderCount = bucket.Count,
                IsCurrent = d.Year == now.Year && d.Month == now.Month
            });
        }

        var current = months.Last();
        var previous = months.Count >= 2 ? months[^2] : new MonthlyRevenueDto();

        var nonZero = months.Where(m => m.Revenue > 0 && !m.IsCurrent).ToList();
        var monthlyAverage = nonZero.Count > 0 ? nonZero.Average(m => m.Revenue) : 0m;

        var best = months.OrderByDescending(m => m.Revenue).FirstOrDefault() ?? new MonthlyRevenueDto();

        var growth = previous.Revenue > 0
            ? Math.Round(((current.Revenue - previous.Revenue) / previous.Revenue) * 100m, 1)
            : 0m;

        // Totales históricos completos (no acotados a 12 meses) — directos en SQL
        var totalRevenue = await _orderRepo.SumDecimalAsync(o => o.Status != "Cancelled", o => o.Total);
        var totalOrders = await _orderRepo.CountAsync(o => o.Status != "Cancelled");
        var averageTicket = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0m;

        var dto = new RevenueProjectionDto
        {
            Months = months,
            TotalRevenue = totalRevenue,
            CurrentMonthRevenue = current.Revenue,
            PreviousMonthRevenue = previous.Revenue,
            MonthlyAverage = Math.Round(monthlyAverage, 2),
            BestMonthRevenue = best.Revenue,
            BestMonthLabel = best.Label,
            TotalOrders = totalOrders,
            AverageTicket = averageTicket,
            GrowthVsPreviousMonth = growth
        };

        return ResponseBase<RevenueProjectionDto>.Ok(dto);
    }
}
