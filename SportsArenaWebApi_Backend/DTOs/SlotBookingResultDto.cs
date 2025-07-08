namespace SportsArenaWebApi_Backend.DTOs
{
    public class SlotBookingResultDto
    {
        public int SlotId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
