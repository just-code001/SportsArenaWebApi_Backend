using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class SportsCategoryDto
    {
        [Required]
        public string CategoryName { get; set; } = null!;
    }
}
