namespace SportsArenaWebApi_Backend.DTOs
{
    public class GenerateVenueSlotsDto
    {
        public int VenueId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }
}
