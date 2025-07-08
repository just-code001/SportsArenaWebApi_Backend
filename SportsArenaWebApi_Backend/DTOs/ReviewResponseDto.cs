namespace SportsArenaWebApi_Backend.DTOs
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
