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
        public class TblvenueslotsController : ControllerBase
        {
            private readonly SportsArenaDbContext _context;

            public TblvenueslotsController(SportsArenaDbContext context)
            {
                _context = context;
            }

            private int? GetProviderIdFromToken()
            {
                var providerId = int.Parse(User.FindFirst("UserId")?.Value!);
                return providerId  ;
            }


            [HttpGet("Admin/all-venueslots")]
            [Authorize(Roles = "admin")]
            public async Task<IActionResult> GetAllSlots([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
            {
                var query = _context.Tblvenueslots.Include(s => s.Venue).AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(s => s.Date >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(s => s.Date <= toDate.Value);

                var slots = await query.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "All slots retrieved.",
                    data = slots
                });
            }

            // ✅ Provider: Get slots by venue ID
            [HttpGet("Provider/venueslots-By-Venue/{venueId}")]
            [Authorize(Roles = "provider")]
            public async Task<IActionResult> GetSlotsByVenueId(int venueId)
            {
                var providerId = GetProviderIdFromToken();
                if (providerId == null) return Unauthorized();

                var venue = await _context.Tblvenues.FindAsync(venueId);
                if (venue == null || venue.ProviderId != providerId) return Forbid();

                var slots = await _context.Tblvenueslots
        .Where(s => s.VenueId == venueId)
        .OrderBy(s => s.Date)
        .ThenBy(s => s.StartTime)
        .Select(s => new GetVenueSlotDto
        {
            SlotId = s.SlotId,
            VenueId = s.VenueId,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsBooked = s.IsBooked
        }).ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Venue slots fetched.",
                    data = slots
                });
            }

            // GET: api/Tblvenueslots
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Tblvenueslot>>> GetTblvenueslots()
            {
                return await _context.Tblvenueslots.ToListAsync();
            }

            // GET: api/Tblvenueslots/5
            [HttpGet("{id}")]
            public async Task<ActionResult<Tblvenueslot>> GetTblvenueslot(int id)
            {
                var tblvenueslot = await _context.Tblvenueslots.FindAsync(id);

                if (tblvenueslot == null)
                {
                    return NotFound();
                }

                return tblvenueslot;
            }

            // PUT: api/Tblvenueslots/5
            [HttpPut("{id}")]
            [Authorize(Roles = "provider")]
            public async Task<IActionResult> UpdateSlot(int id, [FromBody] CreateVenueSlotDto slotDto)
            {
                var providerId = GetProviderIdFromToken();
                if (providerId == null) return Unauthorized();

                var existingSlot = await _context.Tblvenueslots.FindAsync(id);
                if (existingSlot == null) return NotFound();

                if (slotDto.StartTime >= slotDto.EndTime)
                {
                    return BadRequest(new { success = false, message = "StartTime must be less than EndTime." });
                }

                if (existingSlot.IsBooked)
                {
                    return BadRequest(new { success = false, message = "Cannot modify a booked slot." });
                }

                var venue = await _context.Tblvenues.FindAsync(existingSlot.VenueId);
                if (venue == null || venue.ProviderId != providerId) return Forbid();

                bool isOverlapping = await _context.Tblvenueslots.AnyAsync(s =>
                    s.VenueId == existingSlot.VenueId &&
                    s.Date == slotDto.Date &&
                    s.SlotId != id &&
                    (
                        (slotDto.StartTime >= s.StartTime && slotDto.StartTime < s.EndTime) ||
                        (slotDto.EndTime > s.StartTime && slotDto.EndTime <= s.EndTime) ||
                        (slotDto.StartTime <= s.StartTime && slotDto.EndTime >= s.EndTime)
                    ));

                if (isOverlapping)
                {
                    return BadRequest(new { success = false, message = "Updated slot overlaps an existing slot." });
                }

                existingSlot.Date = slotDto.Date;
                existingSlot.StartTime = slotDto.StartTime;
                existingSlot.EndTime = slotDto.EndTime;

                _context.Entry(existingSlot).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Slot updated.", data = existingSlot });
            }

            // POST: api/Tblvenueslots
            // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
            [HttpPost]
            [Authorize(Roles = "provider")]
            public async Task<IActionResult> CreateSlot([FromBody] CreateVenueSlotDto slotDto)
            {
                var providerId = GetProviderIdFromToken();
                if (providerId == null) return Unauthorized();

                var venue = await _context.Tblvenues.FindAsync(slotDto.VenueId);
                if (venue == null || venue.ProviderId != providerId) return Forbid();

                if (slotDto.StartTime >= slotDto.EndTime)
                {
                    return BadRequest(new { success = false, message = "StartTime must be less than EndTime." });
                }

                bool isOverlapping = await _context.Tblvenueslots.AnyAsync(s =>
                    s.VenueId == slotDto.VenueId &&
                    s.Date == slotDto.Date &&
                    (
                        (slotDto.StartTime >= s.StartTime && slotDto.StartTime < s.EndTime) ||
                        (slotDto.EndTime > s.StartTime && slotDto.EndTime <= s.EndTime) ||
                        (slotDto.StartTime <= s.StartTime && slotDto.EndTime >= s.EndTime)
                    ));

                if (isOverlapping)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Slot overlaps with an existing slot."
                    });
                }

                var newSlot = new Tblvenueslot
                {
                    VenueId = slotDto.VenueId,
                    Date = slotDto.Date,
                    StartTime = slotDto.StartTime,
                    EndTime = slotDto.EndTime,
                    IsBooked = false
                };

                _context.Tblvenueslots.Add(newSlot);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Slot created.", data = newSlot });
            }

            // DELETE: api/Tblvenueslots/5
            // ✅ Provider: Delete slot
            [HttpDelete("{id}")]
            [Authorize(Roles = "provider")]
            public async Task<IActionResult> DeleteSlot(int id)
            {
                var providerId = GetProviderIdFromToken();
                if (providerId == null) return Unauthorized();

                var slot = await _context.Tblvenueslots.FindAsync(id);
                if (slot == null) return NotFound();

                if (slot.IsBooked)
                {
                    return BadRequest(new { success = false, message = "Cannot modify a booked slot." });
                }


                var venue = await _context.Tblvenues.FindAsync(slot.VenueId);
                if (venue == null || venue.ProviderId != providerId) return Forbid();

                _context.Tblvenueslots.Remove(slot);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Slot deleted.", data = slot });
            }

            private bool TblvenueslotExists(int id)
            {
                return _context.Tblvenueslots.Any(e => e.SlotId == id);
            }
        }
    }
