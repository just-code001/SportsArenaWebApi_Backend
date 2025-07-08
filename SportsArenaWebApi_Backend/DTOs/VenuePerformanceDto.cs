namespace SportsArenaWebApi_Backend.DTOs
{
    public class VenuePerformanceDto
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int MonthlyBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}
