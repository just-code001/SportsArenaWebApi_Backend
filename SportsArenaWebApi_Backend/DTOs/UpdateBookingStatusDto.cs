using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class UpdateBookingStatusDto
    {
        [Required]
        public string BookingStatus { get; set; } = null!;
    }
}
