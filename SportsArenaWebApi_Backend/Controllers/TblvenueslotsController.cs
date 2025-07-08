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
            return providerId;
        }

        private int? GetClientIdFromToken()
        {
            var clientId = int.Parse(User.FindFirst("UserId")?.Value!);
            return clientId;
        }

        // ✅ Admin: Get all slots with optional date filtering
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

        // POST: api/Tblvenueslots - Create single slot
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
                return BadRequest(new { success = false, message = "Cannot delete a booked slot." });
            }

            var venue = await _context.Tblvenues.FindAsync(slot.VenueId);
            if (venue == null || venue.ProviderId != providerId) return Forbid();

            _context.Tblvenueslots.Remove(slot);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Slot deleted.", data = slot });
        }

        // ✅ Provider: Generate multiple slots automatically
        [HttpPost("Provider/generate-slots")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> GenerateSlots([FromBody] GenerateVenueSlotsDto dto)
        {
            var providerId = GetProviderIdFromToken();
            if (providerId == null) return Unauthorized();

            var venue = await _context.Tblvenues.FindAsync(dto.VenueId);
            if (venue == null || venue.ProviderId != providerId) return Forbid();

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (dto.FromDate > dto.ToDate)
                return BadRequest(new { success = false, message = "FromDate must be before ToDate." });

            if (dto.FromDate < today || dto.ToDate < today)
                return BadRequest(new { success = false, message = "Dates cannot be in the past." });

            // Delete past unbooked slots
            var oldSlots = _context.Tblvenueslots
                .Where(s => s.VenueId == dto.VenueId && s.Date < today && !s.IsBooked);

            _context.Tblvenueslots.RemoveRange(oldSlots);
            await _context.SaveChangesAsync();

            var newSlots = new List<Tblvenueslot>();

            for (var date = dto.FromDate; date <= dto.ToDate; date = date.AddDays(1))
            {
                for (int hour = 9; hour < 24; hour++) // 9 AM to 11 PM
                {
                    var startTime = new TimeOnly(hour, 0);
                    var endTime = startTime.AddHours(1);

                    bool isOverlapping = await _context.Tblvenueslots.AnyAsync(s =>
                        s.VenueId == dto.VenueId &&
                        s.Date == date &&
                        (
                            (startTime >= s.StartTime && startTime < s.EndTime) ||
                            (endTime > s.StartTime && endTime <= s.EndTime) ||
                            (startTime <= s.StartTime && endTime >= s.EndTime)
                        ));

                    if (!isOverlapping)
                    {
                        newSlots.Add(new Tblvenueslot
                        {
                            VenueId = dto.VenueId,
                            Date = date,
                            StartTime = startTime,
                            EndTime = endTime,
                            IsBooked = false
                        });
                    }
                }
            }

            if (newSlots.Any())
            {
                _context.Tblvenueslots.AddRange(newSlots);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = $"{newSlots.Count} slots generated from 9 AM to 12 AM.",
            });
        }

        // ✅ NEW: Admin cleanup past slots
        [HttpPost("Admin/cleanup-past-slots")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CleanupPastSlots()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var pastSlots = await _context.Tblvenueslots
                .Where(s => s.Date < today && !s.IsBooked)
                .ToListAsync();

            if (pastSlots.Any())
            {
                _context.Tblvenueslots.RemoveRange(pastSlots);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = $"Cleaned up {pastSlots.Count} past unbooked slots.",
                deletedCount = pastSlots.Count
            });
        }

        // ✅ ENHANCED: Client get available slots with auto cleanup
        [HttpGet("Client/available-slots/{venueId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetAvailableSlotsForClient(int venueId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var queryFromDate = fromDate ?? today;
            var queryToDate = toDate ?? today.AddDays(30); // Default to next 30 days

            // Automatically clean up past slots for this venue
            var pastSlots = _context.Tblvenueslots
                .Where(s => s.VenueId == venueId && s.Date < today && !s.IsBooked);

            _context.Tblvenueslots.RemoveRange(pastSlots);
            await _context.SaveChangesAsync();

            var slots = await _context.Tblvenueslots
                .Include(s => s.Venue)
                .Where(s => s.VenueId == venueId &&
                           !s.IsBooked &&
                           s.Date >= queryFromDate &&
                           s.Date <= queryToDate)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Select(s => new GetVenueSlotDto
                {
                    SlotId = s.SlotId,
                    VenueId = s.VenueId,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsBooked = s.IsBooked,
                    Priceperhour = s.Venue.Priceperhour
                }).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Available slots retrieved for client.",
                data = slots,
                totalSlots = slots.Count
            });
        }

        // ✅ NEW: Multi-slot booking functionality
        [HttpPost("Client/book-multiple-slots")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> BookMultipleSlots([FromBody] MultiSlotBookingDto bookingDto)
        {
            var clientId = GetClientIdFromToken();
            if (clientId == null) return Unauthorized();

            if (!bookingDto.SlotIds.Any())
            {
                return BadRequest(new { success = false, message = "At least one slot must be selected." });
            }

            var response = new MultiSlotBookingResponseDto();
            var results = new List<SlotBookingResultDto>();
            decimal totalPrice = 0;
            int successCount = 0;
            int failCount = 0;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get all requested slots with venue information
                var requestedSlots = await _context.Tblvenueslots
                    .Include(s => s.Venue)
                    .Where(s => bookingDto.SlotIds.Contains(s.SlotId) && s.VenueId == bookingDto.VenueId)
                    .ToListAsync();

                // Validate that all slots exist and belong to the specified venue
                var missingSlotIds = bookingDto.SlotIds.Except(requestedSlots.Select(s => s.SlotId)).ToList();
                if (missingSlotIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Slots not found or don't belong to venue: {string.Join(", ", missingSlotIds)}"
                    });
                }

                foreach (var slot in requestedSlots)
                {
                    var result = new SlotBookingResultDto
                    {
                        SlotId = slot.SlotId,
                        Price = slot.Venue.Priceperhour
                    };

                    // Check if slot is already booked
                    if (slot.IsBooked)
                    {
                        result.Success = false;
                        result.Message = "Slot is already booked";
                        failCount++;
                    }
                    // Check if slot is in the past
                    else if (slot.Date < DateOnly.FromDateTime(DateTime.Today))
                    {
                        result.Success = false;
                        result.Message = "Cannot book past slots";
                        failCount++;
                    }
                    // Check if slot is too close to current time (e.g., within 1 hour)
                    else if (slot.Date == DateOnly.FromDateTime(DateTime.Today) &&
                             slot.StartTime <= TimeOnly.FromDateTime(DateTime.Now.AddHours(1)))
                    {
                        result.Success = false;
                        result.Message = "Cannot book slots starting within the next hour";
                        failCount++;
                    }
                    else
                    {
                        // Book the slot
                        slot.IsBooked = true;

                        // Here you would typically create a booking record in Tblbooking
                        // For now, we'll just mark the slot as booked

                        result.Success = true;
                        result.Message = "Successfully booked";
                        totalPrice += slot.Venue.Priceperhour;
                        successCount++;
                    }

                    results.Add(result);
                }

                // Save changes if at least one booking was successful
                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }

                response.OverallSuccess = successCount > 0;
                response.Message = successCount > 0
                    ? $"Successfully booked {successCount} out of {bookingDto.SlotIds.Count} slots"
                    : "No slots could be booked";
                response.Results = results;
                response.TotalPrice = totalPrice;
                response.SuccessfulBookings = successCount;
                response.FailedBookings = failCount;

                return Ok(new
                {
                    success = response.OverallSuccess,
                    message = response.Message,
                    data = response
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while booking slots",
                    error = ex.Message
                });
            }
        }

        // ✅ NEW: Get slots with booking status for clients
        [HttpGet("Client/slots-with-status/{venueId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetSlotsWithBookingStatus(int venueId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var queryFromDate = fromDate ?? today;
            var queryToDate = toDate ?? today.AddDays(7); // Default to next 7 days

            var slots = await _context.Tblvenueslots
                .Include(s => s.Venue)
                .Where(s => s.VenueId == venueId &&
                           s.Date >= queryFromDate &&
                           s.Date <= queryToDate)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.SlotId,
                    s.VenueId,
                    s.Date,
                    s.StartTime,
                    s.EndTime,
                    s.IsBooked,
                    Price = s.Venue.Priceperhour,
                    Status = s.IsBooked ? "Booked" :
                            (s.Date < today || (s.Date == today && s.StartTime <= TimeOnly.FromDateTime(DateTime.Now))) ? "Past" : "Available",
                    IsBookable = !s.IsBooked && s.Date >= today &&
                                !(s.Date == today && s.StartTime <= TimeOnly.FromDateTime(DateTime.Now.AddHours(1)))
                }).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Slots with booking status retrieved.",
                data = slots,
                summary = new
                {
                    total = slots.Count,
                    available = slots.Count(s => s.Status == "Available"),
                    booked = slots.Count(s => s.Status == "Booked"),
                    past = slots.Count(s => s.Status == "Past")
                }
            });
        }

        // ✅ NEW: Cancel booking functionality
        [HttpPost("Client/cancel-booking/{slotId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CancelBooking(int slotId)
        {
            var clientId = GetClientIdFromToken();
            if (clientId == null) return Unauthorized();

            var slot = await _context.Tblvenueslots.FindAsync(slotId);
            if (slot == null)
            {
                return NotFound(new { success = false, message = "Slot not found." });
            }

            if (!slot.IsBooked)
            {
                return BadRequest(new { success = false, message = "Slot is not booked." });
            }

            // Check if cancellation is allowed (e.g., at least 2 hours before slot time)
            var slotDateTime = slot.Date.ToDateTime(slot.StartTime);
            if (slotDateTime <= DateTime.Now.AddHours(2))
            {
                return BadRequest(new { success = false, message = "Cannot cancel booking less than 2 hours before slot time." });
            }

            slot.IsBooked = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Booking cancelled successfully.",
                data = slot
            });
        }

        // ✅ NEW: Book single slot (alternative to multi-slot booking)
        [HttpPost("Client/book-single-slot/{slotId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> BookSingleSlot(int slotId)
        {
            var clientId = GetClientIdFromToken();
            if (clientId == null) return Unauthorized();

            var slot = await _context.Tblvenueslots
                .Include(s => s.Venue)
                .FirstOrDefaultAsync(s => s.SlotId == slotId);

            if (slot == null)
            {
                return NotFound(new { success = false, message = "Slot not found." });
            }

            if (slot.IsBooked)
            {
                return BadRequest(new { success = false, message = "Slot is already booked." });
            }

            if (slot.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest(new { success = false, message = "Cannot book past slots." });
            }

            if (slot.Date == DateOnly.FromDateTime(DateTime.Today) &&
                slot.StartTime <= TimeOnly.FromDateTime(DateTime.Now.AddHours(1)))
            {
                return BadRequest(new { success = false, message = "Cannot book slots starting within the next hour." });
            }

            slot.IsBooked = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Slot booked successfully.",
                data = new
                {
                    slot.SlotId,
                    slot.VenueId,
                    slot.Date,
                    slot.StartTime,
                    slot.EndTime,
                    Price = slot.Venue.Priceperhour,
                    VenueName = slot.Venue.Venuename
                }
            });
        }

        private bool TblvenueslotExists(int id)
        {
            return _context.Tblvenueslots.Any(e => e.SlotId == id);
        }
    }
}