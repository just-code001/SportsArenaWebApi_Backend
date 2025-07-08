using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using SportsArenaWebApi_Backend.DTOs;
using SportsArenaWebApi_Backend.Models;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace SportsArenaWebApi_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TblusersController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;
        private readonly IConfiguration _configuration;

        public TblusersController(SportsArenaDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
        }

        // fetch roles -----------
        [HttpGet("fetchroles")]
        public async Task<ActionResult> FetchRoles()
        {
            var roles = await _context.Tblroles.Select(r =>
            new RoleDto { RoleId = r.RoleId, Rolename = r.Rolename }).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "roles fetch successfully.",
                Roles = roles,
            });
        }

        // POST: api/Tblusers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                if (_context.Tblusers.Any(u => u.Email == registerUserDto.Email))
                {
                    throw new Exception("Email Already Exists.");
                }

                var user = new Tbluser
                {
                    Name = registerUserDto.Name,
                    Email = registerUserDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
                    Contact = registerUserDto.Contact,
                    RoleId = registerUserDto.RoleId,
                };

                _context.Tblusers.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Successfully inserted",
                    user = new
                    {
                        user.UserId,
                        user.RoleId,
                        user.Name,
                        user.Email,
                        user.Contact
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // send otp method
        [HttpPost("send-otp")]
        public async Task<ActionResult> SendOtp([FromBody] SendOtpDto sendOtpDto)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("otp", otp);
            HttpContext.Session.SetString("otpEmail", sendOtpDto.Email);

            bool emailSent = await SendEmail(sendOtpDto.Email,otp);

            if (!emailSent)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "failed to send OTP."
                });
            }
            else
            {
                return Ok(new
                {
                    success = true,
                    message = "OTP sent Successfully."
                });
            }
        }

        // email sending method
        private async Task<bool> SendEmail(string email, string otp)
        {
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "OtpTemplate.html");
                string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                emailBody = emailBody.Replace("{{OTP}}", otp);

                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("justcode7755@gmail.com", "hwcq nlws wtua tdkc"),
                    EnableSsl = true,
                };

                var mailMsg = new MailMessage
                {
                    From = new MailAddress("justcode7755@gmail.com"),
                    Subject = "Your OTP code for Registration.",
                    Body = emailBody,
                    IsBodyHtml = true,

                };

                mailMsg.To.Add(email);
                await smtp.SendMailAsync(mailMsg);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // verify otp functionality
        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var sessionOtp = HttpContext.Session.GetString("otp");
            var sessionEmail = HttpContext.Session.GetString("otpEmail");

            if(sessionOtp == null || sessionEmail == null || sessionOtp != verifyOtpDto.Otp || sessionEmail != verifyOtpDto.Email)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Or Expired OTP."
                }); 
            }

            HttpContext.Session.Remove("otp");
            HttpContext.Session.Remove("otpEmail");

            return Ok(new
            {
                success = true,
                message = "OTP Verified Successfully."
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult> login([FromBody] LoginUserDto loginUserDto)
        {
            var user = _context.Tblusers.Include(u => u.Role).Where(u => u.Email == loginUserDto.Email).FirstOrDefault();
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.Password))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Email and Password"
                });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login Successfull.",
                token = token,
                roleId = user.RoleId,
                redirectUrl = GetRedirectUrl(user.RoleId)
            });
        }

        private string GenerateJwtToken(Tbluser user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]); // Load key from config

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim("UserId", user.UserId.ToString()),
        new Claim(ClaimTypes.Role, user.Role.Rolename), // Use ClaimTypes.Role for better compatibility
        new Claim("RoleId", user.RoleId.ToString()),
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"], // Fix audience issue
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("Admin/all-clients")]
        [Authorize(Roles = "admin")] // Only users with Role 'Admin' can access
        public async Task<ActionResult> GetClientUsers()
        {
            try
            {
                var clients = await _context.Tblusers
                    .Where(u => u.RoleId == 3)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Name,
                        u.Email,
                        u.Contact
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Client users fetched successfully.",
                    data = clients
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("Admin/create-provider")]
        [Authorize(Roles = "admin")] // Only Admin can create provider accounts
        public async Task<ActionResult> CreateProvider([FromBody] CreateProviderDto providerDto)
        {
            try
            {
                if (providerDto == null || string.IsNullOrWhiteSpace(providerDto.Email))
                {
                    return BadRequest(new { success = false, message = "Invalid provider data." });
                }

                // Check if email already exists
                if (await _context.Tblusers.AnyAsync(u => u.Email == providerDto.Email))
                {
                    return BadRequest(new { success = false, message = "Email already exists." });
                }

                // Step 1: Generate a random password
                string rawPassword = GenerateRandomPassword();

                // Step 2: Send email with the raw password
                bool emailSent = await SendProviderCredentialsEmail(providerDto.Email, rawPassword);
                if (!emailSent)
                {
                    return StatusCode(500, new { success = false, message = "Failed to send credentials email." });
                }

                // Step 3: Hash the password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

                // Step 4: Create user entity and save
                var user = new Tbluser
                {
                    Name = providerDto.Name,
                    Email = providerDto.Email,
                    Contact = providerDto.Contact,
                    RoleId = 2,
                    Password = hashedPassword
                };

                _context.Tblusers.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Provider created successfully and credentials sent via email.",
                    user = new
                    {
                        user.UserId,
                        user.Name,
                        user.Email,
                        user.Contact,
                        user.RoleId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("Admin/get-providers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllProviders()
        {
            var providers = await _context.Tblusers
                .Where(u => u.RoleId == 2)
                .Select(u => new {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.Contact
                }).ToListAsync();

            return Ok(new { success = true, data = providers });
        }

        [HttpDelete("Admin/delete-provider/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProvider(int id)
        {
            var user = await _context.Tblusers.FindAsync(id);
            if (user == null || user.RoleId != 2)
                return NotFound(new { success = false, message = "Provider not found." });

            _context.Tblusers.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Provider deleted successfully." });
        }




        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> SendProviderCredentialsEmail(string email, string password)
        {
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ProviderCredentialTemplate.html");
                string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                // Replace placeholders
                emailBody = emailBody.Replace("{{Email}}", email)
                                   .Replace("{{Password}}", password);

                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("justcode7755@gmail.com", "hwcq nlws wtua tdkc"),
                    EnableSsl = true,
                };

                var mail = new MailMessage
                {
                    From = new MailAddress("justcode7755@gmail.com"),
                    Subject = "Your Provider Account Credentials",
                    Body = emailBody,
                    IsBodyHtml = true
                };

                mail.To.Add(email);
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        // change password
        [HttpPost("change-password")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                // Get the user ID from JWT token claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { success = false, message = "User ID not found in token." });
                }

                int userId = int.Parse(userIdClaim.Value);

                // Fetch user from database
                var user = await _context.Tblusers.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found." });
                }

                // Verify current (old) password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                {
                    return BadRequest(new { success = false, message = "Current password is incorrect." });
                }

                // Update with new hashed password
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                _context.Tblusers.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal error: {ex.Message}" });
            }
        }

        // forget password 
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Tblusers.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound(new { success = false, message = "Email not found." });

            string token = GeneratePasswordResetToken(user.Email); // Generate a token
            string resetLink = $"http://localhost:3000/reset-password/{token}"; // Include token in reset link

            bool emailSent = await SendResetPasswordEmail(dto.Email, resetLink);
            if (!emailSent)
                return BadRequest(new { success = false, message = "Failed to send reset password email." });

            return Ok(new { success = true, message = "Reset password link sent to email." });
        }

        private string GeneratePasswordResetToken(string email)
        {
            // Set token expiration time (e.g., 15 minutes)
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(15);

            // Combine email and expiration time into a single string
            string tokenPayload = $"{email}|{expirationTime:O}";  // Format to ISO8601 to make it easier to parse later
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenPayload));  // Encode into a Base64 string

            return token;
        }

        private async Task<bool> SendResetPasswordEmail(string email, string resetLink)
        {
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ResetPasswordTemplate.html");
                string htmlBody = await System.IO.File.ReadAllTextAsync(templatePath);
                htmlBody = htmlBody.Replace("{{RESET_LINK}}", resetLink); // Ensure the tokenized link is inserted correctly

                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("justcode7755@gmail.com", "hwcq nlws wtua tdkc"),
                    EnableSsl = true,
                };

                var mail = new MailMessage
                {
                    From = new MailAddress("justcode7755@gmail.com"),
                    Subject = "Reset Your Password",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(email);
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending reset password email: {ex.Message}");
                return false;
            }
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest(new { success = false, message = "Token is required." });
            }

            // Validate the token
            var tokenPayload = ValidateResetPasswordToken(dto.Token);
            if (tokenPayload == null)
            {
                return BadRequest(new { success = false, message = "Invalid or expired token." });
            }

            var user = await _context.Tblusers.FirstOrDefaultAsync(u => u.Email == tokenPayload.Value.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            // Update the password
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword); // Make sure to hash the new password
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password reset successfully." });
        }

        // Method to validate the token
        private (string Email, DateTime Expiration)? ValidateResetPasswordToken(string token)
        {
            try
            {
                // Decode token
                string tokenPayload = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = tokenPayload.Split('|');

                if (parts.Length != 2)
                    return null;

                string email = parts[0];
                DateTime expirationTime = DateTime.Parse(parts[1]);

                if (expirationTime < DateTime.UtcNow)
                {
                    return null; // Token has expired
                }

                return (email, expirationTime);
            }
            catch (Exception)
            {
                return null; // Invalid token format
            }
        }





        private string GetRedirectUrl(int roleId)
        {
            return roleId switch
            {
                1 => "/admin/dashboard",  
                2 => "/provider/dashboard",
                3 => "/client/dashboard",   
                _ => "/" 
            };
        }

        private bool TbluserExists(int id)
        {
            return _context.Tblusers.Any(e => e.UserId == id);
        }
    }
}
