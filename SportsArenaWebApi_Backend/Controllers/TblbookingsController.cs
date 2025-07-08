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
using System.Security.Claims;

namespace SportsArenaWebApi_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TblbookingsController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblbookingsController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblbookings
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Tblbooking>>> GetTblbookings()
        {
            var bookings = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(new { success = true, data = bookings });
        }

        // GET: api/Tblbookings/Admin/dashboard-stats
        [HttpGet("Admin/dashboard-stats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdminDashboardStats()
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);
            var thisYear = new DateTime(now.Year, 1, 1);

            // Basic counts
            var totalBookings = await _context.Tblbookings.CountAsync();
            var totalVenues = await _context.Tblvenues.CountAsync();
            var totalUsers = await _context.Tblusers.Where(u => u.RoleId == 3).CountAsync(); // Clients
            var totalProviders = await _context.Tblusers.Where(u => u.RoleId == 2).CountAsync();

            // Today's stats
            var todayBookings = await _context.Tblbookings
                .Where(b => b.CreatedAt.Date == now.Date)
                .CountAsync();

            var todayRevenue = await _context.Tblpayments
                .Where(p => p.PaymentDate == now.Date && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            // This month stats
            var monthlyBookings = await _context.Tblbookings
                .Where(b => b.CreatedAt >= thisMonth)
                .CountAsync();

            var monthlyRevenue = await _context.Tblpayments
                .Where(p => p.PaymentDate >= thisMonth && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            // Growth calculations
            var lastMonthBookings = await _context.Tblbookings
                .Where(b => b.CreatedAt >= lastMonth && b.CreatedAt < thisMonth)
                .CountAsync();

            var lastMonthRevenue = await _context.Tblpayments
                .Where(p => p.PaymentDate >= lastMonth && p.PaymentDate < thisMonth && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            var bookingGrowth = lastMonthBookings > 0 ?
                ((monthlyBookings - lastMonthBookings) / (decimal)lastMonthBookings) * 100 : 0;

            var revenueGrowth = lastMonthRevenue > 0 ?
                ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

            // Recent bookings
            var recentBookings = await _context.Tblbookings
                .Include(b => b.User)
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new
                {
                    b.BookingId,
                    UserName = b.User.Name,
                    VenueName = b.Slot.Venue.Venuename,
                    b.PayableAmount,
                    b.BookingStatus,
                    b.CreatedAt,
                    SlotDate = b.Slot.Date,
                    SlotTime = $"{b.Slot.StartTime} - {b.Slot.EndTime}"
                })
                .ToListAsync();

            // Monthly booking trends (last 12 months)
            var monthlyTrends = await _context.Tblbookings
                .Where(b => b.CreatedAt >= now.AddMonths(-12))
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(b => b.PayableAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Venue performance
            var venueStats = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Where(b => b.CreatedAt >= thisMonth)
                .GroupBy(b => new { b.Slot.Venue.VenueId, b.Slot.Venue.Venuename })
                .Select(g => new
                {
                    VenueId = g.Key.VenueId,
                    VenueName = g.Key.Venuename,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.PayableAmount)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .ToListAsync();

            // Booking status distribution
            var statusDistribution = await _context.Tblbookings
                .GroupBy(b => b.BookingStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Payment method distribution
            var paymentMethods = await _context.Tblpayments
                .Where(p => p.PaymentStatus == "Success")
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    overview = new
                    {
                        totalBookings,
                        totalVenues,
                        totalUsers,
                        totalProviders,
                        todayBookings,
                        todayRevenue,
                        monthlyBookings,
                        monthlyRevenue,
                        bookingGrowth = Math.Round(bookingGrowth, 2),
                        revenueGrowth = Math.Round(revenueGrowth, 2)
                    },
                    recentBookings,
                    monthlyTrends,
                    venueStats,
                    statusDistribution,
                    paymentMethods
                }
            });
        }

        // GET: api/Tblbookings/Provider/dashboard-stats
        [HttpGet("Provider/dashboard-stats")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> GetProviderDashboardStats()
        {
            var providerId = int.Parse(User.FindFirst("UserId")?.Value!);
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            // Provider's venues
            var providerVenues = await _context.Tblvenues
                .Where(v => v.ProviderId == providerId)
                .Select(v => v.VenueId)
                .ToListAsync();

            if (!providerVenues.Any())
            {
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        overview = new
                        {
                            totalVenues = 0,
                            totalBookings = 0,
                            totalRevenue = 0,
                            monthlyBookings = 0,
                            monthlyRevenue = 0
                        },
                        message = "No venues found for this provider"
                    }
                });
            }

            // Basic counts
            var totalVenues = providerVenues.Count;
            var totalBookings = await _context.Tblbookings
                .Include(b => b.Slot)
                .Where(b => providerVenues.Contains(b.Slot.VenueId))
                .CountAsync();

            var totalRevenue = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Where(p => providerVenues.Contains(p.Booking.Slot.VenueId) && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            // This month stats
            var monthlyBookings = await _context.Tblbookings
                .Include(b => b.Slot)
                .Where(b => providerVenues.Contains(b.Slot.VenueId) && b.CreatedAt >= thisMonth)
                .CountAsync();

            var monthlyRevenue = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Where(p => providerVenues.Contains(p.Booking.Slot.VenueId) &&
                           p.PaymentDate >= thisMonth && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            // Today's stats
            var todayBookings = await _context.Tblbookings
                .Include(b => b.Slot)
                .Where(b => providerVenues.Contains(b.Slot.VenueId) && b.CreatedAt.Date == now.Date)
                .CountAsync();

            var todayRevenue = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Where(p => providerVenues.Contains(p.Booking.Slot.VenueId) &&
                           p.PaymentDate == now.Date && p.PaymentStatus == "Success")
                .SumAsync(p => p.Amount);

            // Growth calculations
            var lastMonthBookings = await _context.Tblbookings
                .Include(b => b.Slot)
                .Where(b => providerVenues.Contains(b.Slot.VenueId) &&
                           b.CreatedAt >= lastMonth && b.CreatedAt < thisMonth)
                .CountAsync();

            var bookingGrowth = lastMonthBookings > 0 ?
                ((monthlyBookings - lastMonthBookings) / (decimal)lastMonthBookings) * 100 : 0;

            // Venue performance
            var venuePerformance = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Where(b => providerVenues.Contains(b.Slot.VenueId))
                .GroupBy(b => new { b.Slot.Venue.VenueId, b.Slot.Venue.Venuename })
                .Select(g => new
                {
                    VenueId = g.Key.VenueId,
                    VenueName = g.Key.Venuename,
                    TotalBookings = g.Count(),
                    MonthlyBookings = g.Count(b => b.CreatedAt >= thisMonth),
                    TotalRevenue = g.Sum(b => b.PayableAmount),
                    MonthlyRevenue = g.Where(b => b.CreatedAt >= thisMonth).Sum(b => b.PayableAmount)
                })
                .OrderByDescending(x => x.TotalBookings)
                .ToListAsync();

            // Recent bookings
            var recentBookings = await _context.Tblbookings
                .Include(b => b.User)
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Where(b => providerVenues.Contains(b.Slot.VenueId))
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new
                {
                    b.BookingId,
                    UserName = b.User.Name,
                    VenueName = b.Slot.Venue.Venuename,
                    b.PayableAmount,
                    b.BookingStatus,
                    b.CreatedAt,
                    SlotDate = b.Slot.Date,
                    SlotTime = $"{b.Slot.StartTime} - {b.Slot.EndTime}"
                })
                .ToListAsync();

            // Monthly trends (last 6 months)
            var monthlyTrends = await _context.Tblbookings
                .Include(b => b.Slot)
                .Where(b => providerVenues.Contains(b.Slot.VenueId) && b.CreatedAt >= now.AddMonths(-6))
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(b => b.PayableAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Slot utilization
            var totalSlots = await _context.Tblvenueslots
                .Where(s => providerVenues.Contains(s.VenueId) && s.Date >= today)
                .CountAsync();

            var bookedSlots = await _context.Tblvenueslots
                .Where(s => providerVenues.Contains(s.VenueId) && s.Date >= today && s.IsBooked)
                .CountAsync();

            var utilizationRate = totalSlots > 0 ? (bookedSlots / (decimal)totalSlots) * 100 : 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    overview = new
                    {
                        totalVenues,
                        totalBookings,
                        totalRevenue,
                        monthlyBookings,
                        monthlyRevenue,
                        todayBookings,
                        todayRevenue,
                        bookingGrowth = Math.Round(bookingGrowth, 2),
                        utilizationRate = Math.Round(utilizationRate, 2)
                    },
                    venuePerformance,
                    recentBookings,
                    monthlyTrends,
                    slotStats = new
                    {
                        totalSlots,
                        bookedSlots,
                        availableSlots = totalSlots - bookedSlots,
                        utilizationRate = Math.Round(utilizationRate, 2)
                    }
                }
            });
        }

        // GET: api/Tblbookings/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetTblbooking(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var booking = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Include(b => b.User)
                .Where(b => b.BookingId == id && b.UserId == userId)
                .Select(b => new GetBookingDto
                {
                    BookingId = b.BookingId,
                    SlotId = b.SlotId,
                    UserId = b.UserId,
                    PayableAmount = b.PayableAmount,
                    PaymentPaid = b.PaymentPaid,
                    BookingStatus = b.BookingStatus,
                    BookingDate = b.BookingDate,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Slot.Date,
                    SlotStartTime = b.Slot.StartTime,
                    SlotEndTime = b.Slot.EndTime,
                    VenueId = b.Slot.VenueId,
                    UserName = b.User.Name,
                    VenueName = b.Slot.Venue.Venuename,
                    VenueLocation = b.Slot.Venue.Location
                })
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound(new { success = false, message = "Booking not found." });

            return Ok(new { success = true, data = booking });
        }

        [HttpGet("Admin/all-bookings")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new GetBookingDto
                {
                    BookingId = b.BookingId,
                    SlotId = b.SlotId,
                    UserId = b.UserId,
                    PayableAmount = b.PayableAmount,
                    PaymentPaid = b.PaymentPaid,
                    BookingStatus = b.BookingStatus,
                    BookingDate = b.BookingDate,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Slot.Date,
                    SlotStartTime = b.Slot.StartTime,
                    SlotEndTime = b.Slot.EndTime,
                    VenueId = b.Slot.VenueId,
                    UserName = b.User.Name,
                    VenueName = b.Slot.Venue.Venuename,
                    VenueLocation = b.Slot.Venue.Location
                })
                .ToListAsync();

            return Ok(new { success = true, data = bookings });
        }

        [HttpGet("Client/my-bookings")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetUserBookings()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);
            var bookings = await _context.Tblbookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                        .ThenInclude(v => v.Category)
                .Include(b => b.Tblpayments)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    BookingId = b.BookingId,
                    SlotId = b.SlotId,
                    UserId = b.UserId,
                    PayableAmount = b.PayableAmount,
                    PaymentPaid = b.PaymentPaid,
                    BookingStatus = b.BookingStatus,
                    BookingDate = b.BookingDate,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Slot.Date,
                    SlotStartTime = b.Slot.StartTime,
                    SlotEndTime = b.Slot.EndTime,
                    VenueId = b.Slot.VenueId,
                    VenueName = b.Slot.Venue.Venuename,
                    VenueLocation = b.Slot.Venue.Location,
                    CategoryName = b.Slot.Venue.Category.Categoryname,
                    // Payment details
                    TransactionId = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.TransactionId : null,
                    PaymentStatus = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.PaymentStatus : null,
                    PaymentDate = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.PaymentDate : null
                })
                .ToListAsync();

            return Ok(new { success = true, data = bookings });
        }

        // Enhanced single booking creation
        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var slot = await _context.Tblvenueslots
                    .Include(s => s.Venue)
                    .FirstOrDefaultAsync(s => s.SlotId == dto.SlotId);

                if (slot == null)
                    return NotFound(new { success = false, message = "Slot not found." });

                if (slot.IsBooked)
                    return BadRequest(new { success = false, message = "Slot is already booked." });

                // Check if slot is in the past
                if (slot.Date < DateOnly.FromDateTime(DateTime.Today))
                    return BadRequest(new { success = false, message = "Cannot book past slots." });

                // Check if slot is too close to current time
                if (slot.Date == DateOnly.FromDateTime(DateTime.Today) &&
                    slot.StartTime <= TimeOnly.FromDateTime(DateTime.Now.AddHours(1)))
                    return BadRequest(new { success = false, message = "Cannot book slots starting within the next hour." });

                var booking = new Tblbooking
                {
                    SlotId = dto.SlotId,
                    UserId = userId,
                    PayableAmount = dto.PayableAmount,
                    PaymentPaid = false,
                    BookingStatus = "Pending",
                    BookingDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tblbookings.Add(booking);
                slot.IsBooked = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Booking created successfully.",
                    data = new
                    {
                        BookingId = booking.BookingId,
                        SlotId = booking.SlotId,
                        UserId = booking.UserId,
                        PayableAmount = booking.PayableAmount,
                        PaymentPaid = booking.PaymentPaid,
                        BookingStatus = booking.BookingStatus,
                        BookingDate = booking.BookingDate,
                        CreatedAt = booking.CreatedAt,
                        Date = slot.Date,
                        SlotStartTime = slot.StartTime,
                        SlotEndTime = slot.EndTime,
                        VenueId = slot.VenueId,
                        VenueName = slot.Venue.Venuename,
                        VenueLocation = slot.Venue.Location
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred while creating booking.", error = ex.Message });
            }
        }

        // NEW: Multi-slot booking creation
        [HttpPost("create-multiple")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CreateMultipleBookings([FromBody] MultiSlotBookingDto bookingDto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            if (!bookingDto.SlotIds.Any())
                return BadRequest(new { success = false, message = "At least one slot must be selected." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var requestedSlots = await _context.Tblvenueslots
                    .Include(s => s.Venue)
                    .Where(s => bookingDto.SlotIds.Contains(s.SlotId) && s.VenueId == bookingDto.VenueId)
                    .ToListAsync();

                var missingSlotIds = bookingDto.SlotIds.Except(requestedSlots.Select(s => s.SlotId)).ToList();
                if (missingSlotIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Slots not found or don't belong to venue: {string.Join(", ", missingSlotIds)}"
                    });
                }

                var results = new List<SlotBookingResultDto>();
                var bookingsToCreate = new List<Tblbooking>();
                var slotsToUpdate = new List<Tblvenueslot>();
                decimal totalAmount = 0;
                int successCount = 0;

                // First pass: validate all slots and prepare bookings
                foreach (var slot in requestedSlots)
                {
                    var result = new SlotBookingResultDto
                    {
                        SlotId = slot.SlotId,
                        Price = slot.Venue.Priceperhour
                    };

                    if (slot.IsBooked)
                    {
                        result.Success = false;
                        result.Message = "Slot is already booked";
                    }
                    else if (slot.Date < DateOnly.FromDateTime(DateTime.Today))
                    {
                        result.Success = false;
                        result.Message = "Cannot book past slots";
                    }
                    else if (slot.Date == DateOnly.FromDateTime(DateTime.Today) &&
                             slot.StartTime <= TimeOnly.FromDateTime(DateTime.Now.AddHours(1)))
                    {
                        result.Success = false;
                        result.Message = "Cannot book slots starting within the next hour";
                    }
                    else
                    {
                        var booking = new Tblbooking
                        {
                            SlotId = slot.SlotId,
                            UserId = userId,
                            PayableAmount = slot.Venue.Priceperhour,
                            PaymentPaid = false,
                            BookingStatus = "Pending",
                            BookingDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        bookingsToCreate.Add(booking);
                        slotsToUpdate.Add(slot);

                        result.Success = true;
                        result.Message = "Successfully booked";
                        totalAmount += slot.Venue.Priceperhour;
                        successCount++;
                    }

                    results.Add(result);
                }

                if (successCount > 0)
                {
                    // Add all bookings to context
                    _context.Tblbookings.AddRange(bookingsToCreate);

                    // Update slot statuses
                    foreach (var slot in slotsToUpdate)
                    {
                        slot.IsBooked = true;
                    }

                    // Save changes to generate booking IDs
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Now create response objects with the generated IDs
                    var createdBookings = bookingsToCreate.Select((booking, index) => new
                    {
                        BookingId = booking.BookingId, // Now this will have the generated ID
                        SlotId = booking.SlotId,
                        UserId = booking.UserId,
                        PayableAmount = booking.PayableAmount,
                        PaymentPaid = booking.PaymentPaid,
                        BookingStatus = booking.BookingStatus,
                        BookingDate = booking.BookingDate,
                        CreatedAt = booking.CreatedAt,
                        Date = slotsToUpdate[index].Date,
                        SlotStartTime = slotsToUpdate[index].StartTime,
                        SlotEndTime = slotsToUpdate[index].EndTime,
                        VenueId = slotsToUpdate[index].VenueId,
                        VenueName = slotsToUpdate[index].Venue.Venuename,
                        VenueLocation = slotsToUpdate[index].Venue.Location
                    }).ToList();

                    var response = new MultiSlotBookingResponseDto
                    {
                        OverallSuccess = true,
                        Message = $"Successfully booked {successCount} out of {bookingDto.SlotIds.Count} slots",
                        Results = results,
                        TotalPrice = totalAmount,
                        SuccessfulBookings = successCount,
                        FailedBookings = bookingDto.SlotIds.Count - successCount
                    };

                    return Ok(new
                    {
                        success = response.OverallSuccess,
                        message = response.Message,
                        data = response,
                        bookings = createdBookings
                    });
                }
                else
                {
                    await transaction.RollbackAsync();

                    var response = new MultiSlotBookingResponseDto
                    {
                        OverallSuccess = false,
                        Message = "No slots could be booked",
                        Results = results,
                        TotalPrice = 0,
                        SuccessfulBookings = 0,
                        FailedBookings = bookingDto.SlotIds.Count
                    };

                    return Ok(new
                    {
                        success = false,
                        message = response.Message,
                        data = response,
                        bookings = new List<object>()
                    });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating bookings",
                    error = ex.Message
                });
            }
        }

        // Cancel booking
        [HttpPost("cancel/{id}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Tblbookings
                    .Include(b => b.Slot)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

                if (booking == null)
                    return NotFound(new { success = false, message = "Booking not found." });

                if (booking.BookingStatus == "Cancelled")
                    return BadRequest(new { success = false, message = "Booking is already cancelled." });

                if (booking.PaymentPaid)
                    return BadRequest(new { success = false, message = "Cannot cancel paid booking. Please contact support." });

                // Check if cancellation is allowed (e.g., at least 2 hours before slot time)
                var slotDateTime = booking.Slot.Date.ToDateTime(booking.Slot.StartTime);
                if (slotDateTime <= DateTime.Now.AddHours(2))
                    return BadRequest(new { success = false, message = "Cannot cancel booking less than 2 hours before slot time." });

                // Update booking status instead of deleting
                booking.BookingStatus = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Slot.IsBooked = false;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Booking cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred while cancelling booking.", error = ex.Message });
            }
        }

        // Provider bookings for their venues
        [HttpGet("Provider/venue-bookings")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> GetProviderVenueBookings()
        {
            var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

            var bookings = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .Include(b => b.User)
                .Where(b => b.Slot.Venue.ProviderId == providerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new GetBookingDto
                {
                    BookingId = b.BookingId,
                    SlotId = b.SlotId,
                    UserId = b.UserId,
                    PayableAmount = b.PayableAmount,
                    PaymentPaid = b.PaymentPaid,
                    BookingStatus = b.BookingStatus,
                    BookingDate = b.BookingDate,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Date = b.Slot.Date,
                    SlotStartTime = b.Slot.StartTime,
                    SlotEndTime = b.Slot.EndTime,
                    VenueId = b.Slot.VenueId,
                    UserName = b.User.Name,
                    VenueName = b.Slot.Venue.Venuename,
                    VenueLocation = b.Slot.Venue.Location
                })
                .ToListAsync();

            return Ok(new { success = true, data = bookings });
        }

        // Add this method to your TblbookingsController to handle multiple booking IDs
        // GET: api/Tblbookings/multiple/{ids}
        [HttpGet("multiple/{ids}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetMultipleBookings(string ids)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value!);

                // Parse the comma-separated booking IDs
                var bookingIds = ids.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.TryParse(id, out int bookingId) ? bookingId : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (!bookingIds.Any())
                {
                    return BadRequest(new { success = false, message = "No valid booking IDs provided" });
                }

                var bookings = await _context.Tblbookings
                    .Include(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                            .ThenInclude(v => v.Category)
                    .Where(b => bookingIds.Contains(b.BookingId) && b.UserId == userId)
                    .Select(b => new MyBookingsResponseDto
                    {
                        BookingId = b.BookingId,
                        VenueName = b.Slot.Venue.Venuename,
                        Location = b.Slot.Venue.Location,
                        CategoryName = b.Slot.Venue.Category.Categoryname,
                        Date = b.Slot.Date.ToDateTime(TimeOnly.MinValue),
                        SlotStartTime = b.Slot.StartTime.ToTimeSpan(),
                        SlotEndTime = b.Slot.EndTime.ToTimeSpan(),
                        PayableAmount = b.PayableAmount,
                        BookingStatus = b.BookingStatus,
                        PaymentPaid = b.PaymentPaid,
                        BookingDate = b.BookingDate,
                        CreatedAt = b.CreatedAt,
                        TransactionId = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.TransactionId : null,
                        PaymentStatus = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.PaymentStatus : null,
                        PaymentDate = b.Tblpayments.FirstOrDefault() != null ? b.Tblpayments.FirstOrDefault()!.PaymentDate : null
                    })
                    .ToListAsync();

                if (!bookings.Any())
                {
                    return NotFound(new { success = false, message = "No bookings found with the provided IDs" });
                }

                return Ok(new { success = true, data = bookings, message = "Bookings retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/Tblbookings/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTblbooking(int id)
        {
            var booking = await _context.Tblbookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound(new { success = false, message = "Booking not found." });

            booking.Slot.IsBooked = false; // release the slot
            _context.Tblbookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Booking deleted successfully." });
        }

        private bool TblbookingExists(int id)
        {
            return _context.Tblbookings.Any(e => e.BookingId == id);
        }
    }
}
