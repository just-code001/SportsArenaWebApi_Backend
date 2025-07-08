using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsArenaWebApi_Backend.DTOs;
using SportsArenaWebApi_Backend.Models;

namespace SportsArenaWebApi_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TblinquiriesController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblinquiriesController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblinquiries
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<InquiryResponseDto>>> GetTblinquiries()
        {
            var inquiries = await _context.Tblinquiries
                .Include(i => i.User)
                .OrderByDescending(i => i.InquiryDate)
                .Select(i => new InquiryResponseDto
                {
                    InquiryId = i.InquiryId,
                    UserId = i.UserId,
                    UserName = i.User.Name,
                    UserEmail = i.User.Email,
                    Message = i.Message,
                    InquiryDate = i.InquiryDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { success = true, data = inquiries });
        }

        // GET: api/Tblinquiries/5
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<InquiryResponseDto>> GetTblinquiry(int id)
        {
            var inquiry = await _context.Tblinquiries
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InquiryId == id);

            if (inquiry == null)
            {
                return NotFound(new { success = false, message = "Inquiry not found." });
            }

            var inquiryDto = new InquiryResponseDto
            {
                InquiryId = inquiry.InquiryId,
                UserId = inquiry.UserId,
                UserName = inquiry.User.Name,
                UserEmail = inquiry.User.Email,
                Message = inquiry.Message,
                InquiryDate = inquiry.InquiryDate ?? DateTime.UtcNow
            };

            return Ok(new { success = true, data = inquiryDto });
        }

        // GET: api/Tblinquiries/user/my-inquiries
        [HttpGet("user/my-inquiries")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<IEnumerable<InquiryResponseDto>>> GetMyInquiries()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var inquiries = await _context.Tblinquiries
                .Include(i => i.User)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.InquiryDate)
                .Select(i => new InquiryResponseDto
                {
                    InquiryId = i.InquiryId,
                    UserId = i.UserId,
                    UserName = i.User.Name,
                    UserEmail = i.User.Email,
                    Message = i.Message,
                    InquiryDate = i.InquiryDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { success = true, data = inquiries });
        }

        // POST: api/Tblinquiries
        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<InquiryResponseDto>> CreateInquiry(CreateInquiryDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var user = await _context.Tblusers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var inquiry = new Tblinquiry
            {
                UserId = userId,
                Message = dto.Message,
                InquiryDate = DateTime.UtcNow
            };

            _context.Tblinquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            var inquiryDto = new InquiryResponseDto
            {
                InquiryId = inquiry.InquiryId,
                UserId = inquiry.UserId,
                UserName = user.Name,
                UserEmail = user.Email,
                Message = inquiry.Message,
                InquiryDate = inquiry.InquiryDate ?? DateTime.UtcNow
            };

            return CreatedAtAction("GetTblinquiry", new { id = inquiry.InquiryId }, new { success = true, data = inquiryDto });
        }

        // DELETE: api/Tblinquiries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,client")]
        public async Task<IActionResult> DeleteTblinquiry(int id)
        {
            var inquiry = await _context.Tblinquiries.FindAsync(id);
            if (inquiry == null)
            {
                return NotFound(new { success = false, message = "Inquiry not found." });
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value!);
            var userRole = User.FindFirst("Role")?.Value;

            // Check if the user is the owner of the inquiry or an admin
            if (inquiry.UserId != userId && userRole != "admin")
            {
                return Forbid();
            }

            _context.Tblinquiries.Remove(inquiry);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inquiry deleted successfully." });
        }

        private bool TblinquiryExists(int id)
        {
            return _context.Tblinquiries.Any(e => e.InquiryId == id);
        }
    }
}
