﻿namespace SportsArenaWebApi_Backend.DTOs
{
    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
