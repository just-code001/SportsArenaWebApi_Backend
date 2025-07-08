namespace SportsArenaWebApi_Backend.DTOs
{
    public class VenueStatsDto
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
