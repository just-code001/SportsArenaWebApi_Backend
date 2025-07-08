namespace SportsArenaWebApi_Backend.DTOs
{
    public class BlogResponseDto
    {
        public int BlogId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public DateTime PublishDate { get; set; }
    }
}
