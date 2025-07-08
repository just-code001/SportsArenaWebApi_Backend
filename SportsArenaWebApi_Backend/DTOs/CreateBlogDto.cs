using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateBlogDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}
