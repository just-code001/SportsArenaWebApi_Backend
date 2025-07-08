using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateProviderDto
    {

        [Required]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string Contact { get; set; } = null!;
    }
}
