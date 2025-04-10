using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer.Localisation;
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
    public class TblvenuesController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblvenuesController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblvenues
        [HttpGet("Provider/my-venues")]
        [Authorize(Roles = "provider")]
        public async Task<ActionResult<IEnumerable<Tblvenue>>> GetTblvenues()
        {
            var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

            var venues = await _context.Tblvenues.Where(v => v.ProviderId == providerId).ToListAsync(); ;
            return Ok(new
            {
                success = true,
                message = "Venues are Fetch Successfully.",
                data = venues   
            });
        }

        // GET: api/Tblvenues/sport-categories
        [HttpGet("sport-categories")]
        [Authorize(Roles = "provider")] // Adjust authorization as needed
        public async Task<ActionResult<IEnumerable<Tblsportcategory>>> GetSportCategories()
        {
            var categories = await _context.Tblsportcategories.ToListAsync();
            return Ok(new
            {
                success = true,
                message = "Sport categories fetched successfully.",
                data = categories
            });
        }

        // GET: api/Tblvenues
        [HttpGet("Admin/all-venue")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Tblvenue>>> GetTblvenuesForAdmin()
        {
            var venues =  await _context.Tblvenues.Include(v => v.Provider).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "All venues are fetched.",
                data = venues   
            });
        }

        // GET: api/Tblvenues/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Tblvenue>> GetTblvenue(int id)
        {
            var tblvenue = await _context.Tblvenues.FindAsync(id);

            if (tblvenue == null)
            {
                return NotFound(new { success = false, message = "Venue not found." });
            }

            // Provider can only view their own venues
            if (User.IsInRole("provider") && tblvenue.ProviderId != int.Parse(User.FindFirst("UserId")?.Value!))
            {
                return Forbid();
            }

            return Ok(new
            {
                success = true,
                message = "Venue found successfully.",
                data = tblvenue
            });
        }

        // PUT: api/Tblvenues/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // PUT: api/Tblvenues/5
        [HttpPut("Provider/update-venue/{id}")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> PutTblvenue(int id, [FromForm] CreateVenueDto updateVenueDto,[FromForm] IFormFile? venueImage)
        {
            try
            {
                var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

                var venue = await _context.Tblvenues.FirstOrDefaultAsync(v => v.VenueId == id && v.ProviderId == providerId);

                if (venue == null)
                {
                    return NotFound(new { success = false, message = "Venue not found or access denied." });
                }

                // If new image is uploaded
                if (venueImage != null && venueImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(venueImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { success = false, message = "Invalid image format. Only .jpg, .jpeg, .png allowed." });
                    }

                    const int maxFileSize = 2 * 1024 * 1024;
                    if (venueImage.Length > maxFileSize)
                    {
                        return BadRequest(new { success = false, message = "Image size must be less than 2MB." });
                    }

                    string uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure path exists
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await venueImage.CopyToAsync(stream);
                    }

                    // Optionally delete the old image file here

                    venue.VenueImage = uniqueFileName;
                }

                // Update the venue properties
                venue.CategoryId = updateVenueDto.CategoryId;
                venue.Venuename = updateVenueDto.Venuename;
                venue.Location = updateVenueDto.Location;
                venue.Description = updateVenueDto.Description;
                venue.Capacity = updateVenueDto.Capacity;
                venue.Priceperhour = updateVenueDto.PricePerHour;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Venue updated successfully.",
                    data = venue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }


        // POST: api/Tblvenues
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "provider")]
        public async Task<ActionResult<Tblvenue>> PostTblvenue([FromForm] CreateVenueDto createVenueDto,[FromForm] IFormFile venueImage)
        {
            try
            {
                if (venueImage == null || venueImage.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Please upload an image." });
                }

                // Validate Image Format
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(venueImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { success = false, message = "Invalid image format. Only .jpg, .jpeg, .png allowed." });
                }

                // Validate Image Size (Max 2MB)
                const int maxFileSize = 2 * 1024 * 1024; // 2MB
                if (venueImage.Length > maxFileSize)
                {
                    return BadRequest(new { success = false, message = "Image size must be less than 2MB." });
                }

                // Generate Image Name Using Only Timestamp
                string uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

                // Set Image Save Path (Adjust this path as needed)
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save Image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await venueImage.CopyToAsync(stream);
                }

                int providerId = int.Parse(User.FindFirst("UserId")?.Value!);

                var venue = new Tblvenue
                {
                    ProviderId = providerId,
                    CategoryId = createVenueDto.CategoryId,
                    Venuename = createVenueDto.Venuename,
                    Location = createVenueDto.Location,
                    Description = createVenueDto.Description,
                    Capacity = createVenueDto.Capacity,
                    Priceperhour = createVenueDto.PricePerHour,
                    IsActive = true,
                    VenueImage = uniqueFileName
                };

                _context.Tblvenues.Add(venue);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Venue added successfully.",
                    data = venue
                });
            }
            catch (Exception ex) {
                return StatusCode(500, new { success = false, message = "Internal Server Error.", error = ex.Message });
            }
        }

        // DELETE: api/Tblvenues/5
        // DELETE: api/Tblvenues/5
        [HttpDelete("Provider/delete-venue/{id}")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> DeleteTblvenue(int id)
        {
            try
            {
                var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

                var venue = await _context.Tblvenues.FirstOrDefaultAsync(v => v.VenueId == id && v.ProviderId == providerId);

                if (venue == null)
                {
                    return NotFound(new { success = false, message = "Venue not found or access denied." });
                }

                _context.Tblvenues.Remove(venue);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Venue deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }


        private bool TblvenueExists(int id)
        {
            return _context.Tblvenues.Any(e => e.VenueId == id);
        }
    }
}
