namespace SportsArenaWebApi_Backend.DTOs
{
    public class GetPaymentDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string TransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public DateTime? PaymentDate { get; set; } // Nullable to match model
        public string? PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserName { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueLocation { get; set; } = null!;
        public DateOnly SlotDate { get; set; }
        public TimeOnly SlotStartTime { get; set; }
        public TimeOnly SlotEndTime { get; set; }
    }
}
