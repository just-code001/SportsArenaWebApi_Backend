namespace SportsArenaWebApi_Backend.DTOs
{
    public class RecentBookingDto
    {
        public int BookingId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public decimal PayableAmount { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateOnly SlotDate { get; set; }
        public string SlotTime { get; set; } = string.Empty;
    }
}
