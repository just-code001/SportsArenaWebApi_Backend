namespace SportsArenaWebApi_Backend.DTOs
{
    public class GetBookingDto
    {
        public int BookingId { get; set; }
        public int SlotId { get; set; }
        public int UserId { get; set; }
        public decimal PayableAmount { get; set; }
        public bool PaymentPaid { get; set; }
        public string BookingStatus { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly SlotStartTime { get; set; }
        public TimeOnly SlotEndTime { get; set; }
        public int VenueId { get; set; }
        public string? UserName { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueLocation { get; set; } = null!;
    }
}
