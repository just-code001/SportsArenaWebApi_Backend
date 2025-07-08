namespace SportsArenaWebApi_Backend.DTOs
{
    public class InquiryResponseDto
    {
        public int InquiryId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime InquiryDate { get; set; }
    }
}
