using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        [Required, Phone]
        public string Contact { get; set; } = null!;
    }
}
