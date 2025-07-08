namespace SportsArenaWebApi_Backend.DTOs
{
    public class ProviderOverviewStatsDto
    {
        public int TotalVenues { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int MonthlyBookings { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TodayBookings { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal BookingGrowth { get; set; }
        public decimal UtilizationRate { get; set; }
    }
}
