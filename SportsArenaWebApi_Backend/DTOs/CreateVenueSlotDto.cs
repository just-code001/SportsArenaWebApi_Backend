namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateVenueSlotDto
    {
        public int VenueId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
