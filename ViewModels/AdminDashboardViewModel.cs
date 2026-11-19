namespace warat_0583_l005.ViewModels;

public class AdminDashboardViewModel
{
    public string AdminName { get; set; } = "Admin";
    public int TotalUsers { get; set; }
    public int TotalBookings { get; set; }
    public int TotalCars { get; set; }
    public int PendingPayments { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CurrentMonthSales { get; set; }
    public List<string> SalesMonthLabels { get; set; } = [];
    public List<decimal> MonthlySales { get; set; } = [];
    public List<string> TopCarLabels { get; set; } = [];
    public List<decimal> TopCarSales { get; set; } = [];
    public List<string> BookingStatusLabels { get; set; } = [];
    public List<int> BookingStatusCounts { get; set; } = [];
}
