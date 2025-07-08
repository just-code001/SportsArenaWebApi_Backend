namespace SportsArenaWebApi_Backend.DTOs
{
    public class ProviderDashboardStatsDto
    {
        public ProviderOverviewStatsDto Overview { get; set; } = new();
        public List<VenuePerformanceDto> VenuePerformance { get; set; } = new();
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public SlotStatsDto SlotStats { get; set; } = new();
    }
}
