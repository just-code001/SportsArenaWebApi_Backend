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
    public class TblpaymentsController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblpaymentsController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblpayments
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Tblpayment>>> GetTblpayments()
        {
            var payments = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                .Include(p => p.Booking.User)
                .OrderByDescending(p => p.CreatedAt) // Use CreatedAt instead of PaymentDate for ordering
                .ToListAsync();

            return Ok(new { success = true, data = payments });
        }

        // GET: api/Tblpayments/5
        [HttpGet("{id}")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<Tblpayment>> GetTblpayment(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var payment = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                .Include(p => p.Booking.User)
                .Where(p => p.PaymentId == id && p.Booking.UserId == userId)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                return NotFound(new { success = false, message = "Payment not found." });
            }

            var paymentDto = new GetPaymentDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate, // Now this works since both are DateTime
                PaymentMethod = payment.PaymentMethod,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                VenueName = payment.Booking.Slot.Venue.Venuename,
                VenueLocation = payment.Booking.Slot.Venue.Location,
                SlotDate = payment.Booking.Slot.Date,
                SlotStartTime = payment.Booking.Slot.StartTime,
                SlotEndTime = payment.Booking.Slot.EndTime
            };

            return Ok(new { success = true, data = paymentDto });
        }

        [HttpGet("Admin/all-payments")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                .Include(p => p.Booking.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new GetPaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    UserName = p.Booking.User.Name,
                    VenueName = p.Booking.Slot.Venue.Venuename,
                    VenueLocation = p.Booking.Slot.Venue.Location,
                    SlotDate = p.Booking.Slot.Date,
                    SlotStartTime = p.Booking.Slot.StartTime,
                    SlotEndTime = p.Booking.Slot.EndTime
                })
                .ToListAsync();

            return Ok(new { success = true, data = payments });
        }

        [HttpGet("Client/my-payments")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetUserPayments()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var payments = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                .Where(p => p.Booking.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new GetPaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    VenueName = p.Booking.Slot.Venue.Venuename,
                    VenueLocation = p.Booking.Slot.Venue.Location,
                    SlotDate = p.Booking.Slot.Date,
                    SlotStartTime = p.Booking.Slot.StartTime,
                    SlotEndTime = p.Booking.Slot.EndTime
                })
                .ToListAsync();

            return Ok(new { success = true, data = payments });
        }

        [HttpGet("Provider/venue-payments")]
        [Authorize(Roles = "provider")]
        public async Task<IActionResult> GetProviderVenuePayments()
        {
            var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

            var payments = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                .Include(p => p.Booking.User)
                .Where(p => p.Booking.Slot.Venue.ProviderId == providerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new GetPaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    UserName = p.Booking.User.Name,
                    VenueName = p.Booking.Slot.Venue.Venuename,
                    VenueLocation = p.Booking.Slot.Venue.Location,
                    SlotDate = p.Booking.Slot.Date,
                    SlotStartTime = p.Booking.Slot.StartTime,
                    SlotEndTime = p.Booking.Slot.EndTime
                })
                .ToListAsync();

            return Ok(new { success = true, data = payments });
        }

        // Process payment
        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var booking = await _context.Tblbookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId && b.UserId == userId);

            if (booking == null)
                return NotFound(new { success = false, message = "Booking not found." });

            if (booking.PaymentPaid)
                return BadRequest(new { success = false, message = "Payment already completed for this booking." });

            // Validate payment amount
            if (dto.Amount != booking.PayableAmount)
                return BadRequest(new { success = false, message = "Payment amount does not match booking amount." });

            // Check for duplicate transaction ID
            var existingPayment = await _context.Tblpayments
                .FirstOrDefaultAsync(p => p.TransactionId == dto.TransactionId);

            if (existingPayment != null)
                return BadRequest(new { success = false, message = "Payment with this transaction ID already exists." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = new Tblpayment
                {
                    BookingId = dto.BookingId,
                    TransactionId = dto.TransactionId,
                    Amount = dto.Amount,
                    PaymentStatus = dto.PaymentStatus,
                    PaymentDate = DateTime.UtcNow, // Set current time
                    PaymentMethod = dto.PaymentMethod ?? "Razorpay",
                    PaymentGatewayResponse = dto.PaymentGatewayResponse,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tblpayments.Add(payment);

                // Update booking status based on payment status
                if (dto.PaymentStatus.ToLower() == "success")
                {
                    booking.PaymentPaid = true;
                    booking.BookingStatus = "Confirmed";
                    booking.UpdatedAt = DateTime.UtcNow;
                }
                else if (dto.PaymentStatus.ToLower() == "failed")
                {
                    booking.BookingStatus = "Cancelled";
                    booking.UpdatedAt = DateTime.UtcNow;
                    booking.Slot.IsBooked = false; // Release the slot
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var paymentDto = new GetPaymentDto
                {
                    PaymentId = payment.PaymentId,
                    BookingId = payment.BookingId,
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    PaymentStatus = payment.PaymentStatus,
                    PaymentDate = payment.PaymentDate,
                    PaymentMethod = payment.PaymentMethod,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt,
                    VenueName = booking.Slot.Venue.Venuename,
                    VenueLocation = booking.Slot.Venue.Location,
                    SlotDate = booking.Slot.Date,
                    SlotStartTime = booking.Slot.StartTime,
                    SlotEndTime = booking.Slot.EndTime
                };

                return Ok(new
                {
                    success = true,
                    message = "Payment processed successfully.",
                    data = paymentDto
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing payment.",
                    error = ex.Message
                });
            }
        }

        // Add this method to your TblpaymentsController to handle multiple payments
        [HttpPost("multiple")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CreateMultiplePayments([FromBody] CreateMultiplePaymentsDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value!);

                if (dto.BookingIds == null || !dto.BookingIds.Any())
                {
                    return BadRequest(new { success = false, message = "No booking IDs provided." });
                }

                // Check for duplicate transaction ID first
                var existingPayment = await _context.Tblpayments
                    .FirstOrDefaultAsync(p => p.TransactionId == dto.TransactionId);

                if (existingPayment != null)
                {
                    return BadRequest(new { success = false, message = "Payment with this transaction ID already exists." });
                }

                // Fetch all bookings in a separate query to avoid issues with tracking
                var bookings = await _context.Tblbookings
                    .Include(b => b.Slot)
                        .ThenInclude(s => s.Venue)
                    .Where(b => dto.BookingIds.Contains(b.BookingId) && b.UserId == userId)
                    .ToListAsync();

                if (!bookings.Any())
                {
                    return NotFound(new { success = false, message = "No bookings found with the provided IDs." });
                }

                // Check if any booking is already paid
                var paidBookings = bookings.Where(b => b.PaymentPaid).ToList();
                if (paidBookings.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Payment already completed for booking(s): {string.Join(", ", paidBookings.Select(b => b.BookingId))}."
                    });
                }

                // Validate total payment amount
                var totalBookingAmount = bookings.Sum(b => b.PayableAmount);
                if (dto.TotalAmount != totalBookingAmount)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Payment amount ({dto.TotalAmount}) does not match total booking amount ({totalBookingAmount})."
                    });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var payments = new List<Tblpayment>();
                    var now = DateTime.UtcNow;

                    foreach (var booking in bookings)
                    {
                        var payment = new Tblpayment
                        {
                            BookingId = booking.BookingId,
                            TransactionId = dto.TransactionId,
                            Amount = booking.PayableAmount,
                            PaymentStatus = dto.PaymentStatus,
                            PaymentDate = now,
                            PaymentMethod = dto.PaymentMethod ?? "Razorpay",
                            PaymentGatewayResponse = dto.PaymentGatewayResponse,
                            CreatedAt = now
                        };

                        _context.Tblpayments.Add(payment);
                        payments.Add(payment);

                        // Update booking status based on payment status
                        if (dto.PaymentStatus.ToLower() == "success")
                        {
                            booking.PaymentPaid = true;
                            booking.BookingStatus = "Confirmed";
                            booking.UpdatedAt = now;
                        }
                        else if (dto.PaymentStatus.ToLower() == "failed")
                        {
                            booking.BookingStatus = "Cancelled";
                            booking.UpdatedAt = now;
                            booking.Slot.IsBooked = false; // Release the slot
                        }
                    }

                    // Save changes
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Create response DTOs
                    var paymentDtos = payments.Select(p => new
                    {
                        PaymentId = p.PaymentId,
                        BookingId = p.BookingId,
                        TransactionId = p.TransactionId,
                        Amount = p.Amount,
                        PaymentStatus = p.PaymentStatus,
                        PaymentDate = p.PaymentDate,
                        PaymentMethod = p.PaymentMethod,
                        CreatedAt = p.CreatedAt
                    }).ToList();

                    return Ok(new
                    {
                        success = true,
                        message = $"Successfully processed payment for {bookings.Count} bookings.",
                        data = paymentDtos
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Log the inner exception for debugging
                    var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while processing payments.",
                        error = ex.Message,
                        innerError = innerExceptionMessage
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the request.",
                    error = ex.Message
                });
            }
        }

        // Refund payment (admin only)
        [HttpPost("refund/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RefundPayment(int id)
        {
            var payment = await _context.Tblpayments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound(new { success = false, message = "Payment not found." });

            if (payment.PaymentStatus == "Refunded")
                return BadRequest(new { success = false, message = "Payment already refunded." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                payment.PaymentStatus = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Booking.PaymentPaid = false;
                payment.Booking.BookingStatus = "Cancelled";
                payment.Booking.UpdatedAt = DateTime.UtcNow;
                payment.Booking.Slot.IsBooked = false; // Release the slot

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Payment refunded successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing refund.",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Tblpayments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTblpayment(int id)
        {
            var payment = await _context.Tblpayments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound(new { success = false, message = "Payment not found." });
            }

            // Reset booking payment status
            payment.Booking.PaymentPaid = false;
            payment.Booking.BookingStatus = "Cancelled";
            payment.Booking.UpdatedAt = DateTime.UtcNow;

            _context.Tblpayments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Payment deleted successfully." });
        }

        private bool TblpaymentExists(int id)
        {
            return _context.Tblpayments.Any(e => e.PaymentId == id);
        }
    }
}
