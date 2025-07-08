namespace SportsArenaWebApi_Backend.DTOs
{
    public class OverviewStatsDto
    {
        public int TotalBookings { get; set; }
        public int TotalVenues { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int TodayBookings { get; set; }
        public decimal TodayRevenue { get; set; }
        public int MonthlyBookings { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal BookingGrowth { get; set; }
        public decimal RevenueGrowth { get; set; }
    }
}
