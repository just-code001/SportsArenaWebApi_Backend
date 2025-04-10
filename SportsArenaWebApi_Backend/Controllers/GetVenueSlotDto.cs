namespace SportsArenaWebApi_Backend.Controllers
{
    public class GetVenueSlotDto
    {
        public int SlotId { get; set; }
        public int VenueId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBooked { get; set; }
    }
}
