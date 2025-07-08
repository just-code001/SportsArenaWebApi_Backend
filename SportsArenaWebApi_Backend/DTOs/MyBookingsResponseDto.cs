namespace SportsArenaWebApi_Backend.DTOs
{
    public class MyBookingsResponseDto
    {
        public int BookingId { get; set; }
        public string VenueName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public DateTime Date { get; set; }
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public decimal PayableAmount { get; set; }
        public string BookingStatus { get; set; } = null!;
        public bool PaymentPaid { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Payment details
        public string? TransactionId { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
