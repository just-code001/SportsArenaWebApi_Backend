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


        // GET: api/Tblusers
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Tbluser>>> GetTblusers()
        //{
        //    return await _context.Tblusers.ToListAsync();
        //}

        //// GET: api/Tblusers/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Tbluser>> GetTbluser(int id)
        //{
        //    var tbluser = await _context.Tblusers.FindAsync(id);

        //    if (tbluser == null)
        //    {
        //        return NotFound();
        //    }

        //    return tbluser;
        //}

        //// PUT: api/Tblusers/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTbluser(int id, Tbluser tbluser)
        //{
        //    if (id != tbluser.UserId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tbluser).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TbluserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

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

        // DELETE: api/Tblusers/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTbluser(int id)
        //{
        //    var tbluser = await _context.Tblusers.FindAsync(id);
        //    if (tbluser == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Tblusers.Remove(tbluser);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

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
