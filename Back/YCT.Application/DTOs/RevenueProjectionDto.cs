namespace YCT.Application.DTOs;

public class RevenueProjectionDto
{
    public List<MonthlyRevenueDto> Months { get; set; } = new();

    public decimal TotalRevenue { get; set; }
    public decimal CurrentMonthRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal MonthlyAverage { get; set; }
    public decimal BestMonthRevenue { get; set; }
    public string BestMonthLabel { get; set; } = string.Empty;

    public int TotalOrders { get; set; }
    public decimal AverageTicket { get; set; }
    public decimal GrowthVsPreviousMonth { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public bool IsCurrent { get; set; }
}
