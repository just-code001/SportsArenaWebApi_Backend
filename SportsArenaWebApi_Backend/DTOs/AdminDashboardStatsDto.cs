namespace SportsArenaWebApi_Backend.DTOs
{
    public class AdminDashboardStatsDto
    {
        public OverviewStatsDto Overview { get; set; } = new();
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public List<VenueStatsDto> VenueStats { get; set; } = new();
        public List<StatusDistributionDto> StatusDistribution { get; set; } = new();
        public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
    }
}
