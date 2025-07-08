using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class MultiSlotBookingDto
    {
        [Required]
        public List<int> SlotIds { get; set; } = new List<int>();

        [Required]
        public int VenueId { get; set; }
    }
}
