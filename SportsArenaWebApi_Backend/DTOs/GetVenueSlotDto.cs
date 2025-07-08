namespace SportsArenaWebApi_Backend.DTOs
{
    public class GetVenueSlotDto
    {
        public int SlotId { get; set; }
        public int VenueId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsBooked { get; set; }

        public decimal Priceperhour { get; set; }
    }
}
