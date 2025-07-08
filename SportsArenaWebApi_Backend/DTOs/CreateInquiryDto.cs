using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateInquiryDto
    {
        [Required]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = null!;
    }
}
