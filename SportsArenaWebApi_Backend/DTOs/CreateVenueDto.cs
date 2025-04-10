using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateVenueDto
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string Venuename { get; set; } = null!;
        [Required]
        public string Location { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public int Capacity { get; set; }
        [Required]
        public decimal PricePerHour { get; set; }
        [Required]
        public IFormFile VenueImage { get; set; } = null!;
    }
}
