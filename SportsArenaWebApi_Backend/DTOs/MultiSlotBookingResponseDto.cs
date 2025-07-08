namespace SportsArenaWebApi_Backend.DTOs
{
    public class MultiSlotBookingResponseDto
    {
        public bool OverallSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SlotBookingResultDto> Results { get; set; } = new List<SlotBookingResultDto>();
        public decimal TotalPrice { get; set; }
        public int SuccessfulBookings { get; set; }
        public int FailedBookings { get; set; }
    }
}
