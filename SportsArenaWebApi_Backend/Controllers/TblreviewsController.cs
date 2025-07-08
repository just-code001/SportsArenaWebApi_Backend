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
    public class TblreviewsController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblreviewsController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblreviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetTblreviews()
        {
            var reviews = await _context.Tblreviews
                .Include(r => r.User)
                .Include(r => r.Venue)
                .OrderByDescending(r => r.ReviewId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                VenueId = r.VenueId,
                VenueName = r.Venue.Venuename,
                UserId = r.UserId,
                UserName = r.User.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                // Since your model might not have these fields, using default values
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }).ToList();

            return Ok(new { success = true, data = reviewDtos });
        }

        // GET: api/Tblreviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponseDto>> GetTblreview(int id)
        {
            var review = await _context.Tblreviews
                .Include(r => r.User)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            var reviewDto = new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                VenueId = review.VenueId,
                VenueName = review.Venue.Venuename,
                UserId = review.UserId,
                UserName = review.User.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            return Ok(new { success = true, data = reviewDto });
        }

        // GET: api/Tblreviews/venue/5
        [HttpGet("venue/{venueId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviewsByVenue(int venueId)
        {
            var venue = await _context.Tblvenues.FindAsync(venueId);
            if (venue == null)
            {
                return NotFound(new { success = false, message = "Venue not found." });
            }

            var reviews = await _context.Tblreviews
                .Include(r => r.User)
                .Include(r => r.Venue)
                .Where(r => r.VenueId == venueId)
                .OrderByDescending(r => r.ReviewId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                VenueId = r.VenueId,
                VenueName = r.Venue.Venuename,
                UserId = r.UserId,
                UserName = r.User.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }).ToList();

            // Calculate average rating - add explicit cast to decimal
            var averageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0m;

            return Ok(new
            {
                success = true,
                data = reviewDtos,
                averageRating = averageRating,
                totalReviews = reviews.Count
            });
        }

        // GET: api/Tblreviews/user/my-reviews
        [HttpGet("user/my-reviews")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetMyReviews()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var reviews = await _context.Tblreviews
                .Include(r => r.User)
                .Include(r => r.Venue)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReviewId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                VenueId = r.VenueId,
                VenueName = r.Venue.Venuename,
                UserId = r.UserId,
                UserName = r.User.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }).ToList();

            return Ok(new { success = true, data = reviewDtos });
        }

        // GET: api/Tblreviews/can-review/{venueId}
        [HttpGet("can-review/{venueId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CanReviewVenue(int venueId)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            // Check if venue exists
            var venue = await _context.Tblvenues.FindAsync(venueId);
            if (venue == null)
            {
                return NotFound(new { success = false, message = "Venue not found." });
            }

            // Check if user has already reviewed this venue
            var existingReview = await _context.Tblreviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VenueId == venueId);

            if (existingReview != null)
            {
                return Ok(new { success = true, canReview = false, message = "You have already reviewed this venue." });
            }

            // Check if user has booked this venue before
            var hasBooking = await _context.Tblbookings
                .Include(b => b.Slot)
                .AnyAsync(b => b.UserId == userId && b.Slot.VenueId == venueId);

            return Ok(new
            {
                success = true,
                canReview = hasBooking,
                message = hasBooking ?
                    "You can review this venue." :
                    "You can only review venues that you have booked."
            });
        }

        // POST: api/Tblreviews
        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<ReviewResponseDto>> CreateReview(CreateReviewDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            // Check if venue exists
            var venue = await _context.Tblvenues.FindAsync(dto.VenueId);
            if (venue == null)
            {
                return NotFound(new { success = false, message = "Venue not found." });
            }

            // Check if user has already reviewed this venue
            var existingReview = await _context.Tblreviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VenueId == dto.VenueId);

            if (existingReview != null)
            {
                return BadRequest(new { success = false, message = "You have already reviewed this venue. Please update your existing review." });
            }

            // Check if user has booked this venue before (optional validation)
            var hasBooking = await _context.Tblbookings
                .Include(b => b.Slot)
                .AnyAsync(b => b.UserId == userId && b.Slot.Venue.VenueId == dto.VenueId);

            if (!hasBooking)
            {
                return BadRequest(new { success = false, message = "You can only review venues that you have booked." });
            }

            var review = new Tblreview
            {
                VenueId = dto.VenueId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Tblreviews.Add(review);
            await _context.SaveChangesAsync();

            var user = await _context.Tblusers.FindAsync(userId);

            var reviewDto = new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                VenueId = review.VenueId,
                VenueName = venue.Venuename,
                UserId = review.UserId,
                UserName = user!.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            // Calculate new average rating
            var averageRating = await CalculateVenueAverageRating(dto.VenueId);

            return CreatedAtAction("GetTblreview", new { id = review.ReviewId }, new
            {
                success = true,
                data = reviewDto,
                venueAverageRating = averageRating
            });
        }

        // PUT: api/Tblreviews/5
        [HttpPut("{id}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var review = await _context.Tblreviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            // Check if the review belongs to the user
            if (review.UserId != userId)
            {
                return Forbid();
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            try
            {
                await _context.SaveChangesAsync();

                // Calculate new average rating
                var averageRating = await CalculateVenueAverageRating(review.VenueId);

                return Ok(new
                {
                    success = true,
                    message = "Review updated successfully.",
                    venueAverageRating = averageRating
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblreviewExists(id))
                {
                    return NotFound(new { success = false, message = "Review not found." });
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Tblreviews/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "client,admin")]
        public async Task<IActionResult> DeleteTblreview(int id)
        {
            var review = await _context.Tblreviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value!);
            var userRole = User.FindFirst("Role")?.Value;

            // Check if the user is the owner of the review or an admin
            if (review.UserId != userId && userRole != "admin")
            {
                return Forbid();
            }

            var venueId = review.VenueId;

            _context.Tblreviews.Remove(review);
            await _context.SaveChangesAsync();

            // Calculate new average rating
            var averageRating = await CalculateVenueAverageRating(venueId);

            return Ok(new
            {
                success = true,
                message = "Review deleted successfully.",
                venueAverageRating = averageRating
            });
        }

        // GET: api/Tblreviews/venue-stats/{venueId}
        [HttpGet("venue-stats/{venueId}")]
        public async Task<IActionResult> GetVenueReviewStats(int venueId)
        {
            var venue = await _context.Tblvenues.FindAsync(venueId);
            if (venue == null)
            {
                return NotFound(new { success = false, message = "Venue not found." });
            }

            var reviews = await _context.Tblreviews
                .Where(r => r.VenueId == venueId)
                .ToListAsync();

            var totalReviews = reviews.Count;
            // Add explicit cast to decimal
            var averageRating = totalReviews > 0 ? (decimal)reviews.Average(r => r.Rating) : 0m;

            // Calculate rating distribution
            var ratingDistribution = new int[5];
            foreach (var review in reviews)
            {
                ratingDistribution[review.Rating - 1]++;
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    venueId = venueId,
                    venueName = venue.Venuename,
                    totalReviews = totalReviews,
                    averageRating = averageRating,
                    ratingDistribution = new
                    {
                        oneStar = ratingDistribution[0],
                        twoStars = ratingDistribution[1],
                        threeStars = ratingDistribution[2],
                        fourStars = ratingDistribution[3],
                        fiveStars = ratingDistribution[4]
                    }
                }
            });
        }

        // GET: api/Tblreviews/provider/venue-reviews
        [HttpGet("provider/venue-reviews")]
        [Authorize(Roles = "provider")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetProviderVenueReviews()
        {
            var providerId = int.Parse(User.FindFirst("UserId")?.Value!);

            // Get all venues owned by this provider
            var providerVenues = await _context.Tblvenues
                .Where(v => v.ProviderId == providerId)
                .Select(v => v.VenueId)
                .ToListAsync();

            if (!providerVenues.Any())
            {
                return Ok(new { success = true, data = new List<ReviewResponseDto>() });
            }

            // Get all reviews for these venues
            var reviews = await _context.Tblreviews
                .Include(r => r.User)
                .Include(r => r.Venue)
                .Where(r => providerVenues.Contains(r.VenueId))
                .OrderByDescending(r => r.ReviewId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                VenueId = r.VenueId,
                VenueName = r.Venue.Venuename,
                UserId = r.UserId,
                UserName = r.User.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }).ToList();

            // Calculate overall statistics
            decimal overallAverageRating = 0;
            if (reviews.Any())
            {
                overallAverageRating = (decimal)reviews.Average(r => r.Rating);
            }

            // Calculate per-venue statistics
            var venueStats = providerVenues
                .Select(venueId => new
                {
                    VenueId = venueId,
                    VenueName = _context.Tblvenues.FirstOrDefault(v => v.VenueId == venueId)?.Venuename ?? "Unknown",
                    Reviews = reviews.Where(r => r.VenueId == venueId).ToList(),
                    AverageRating = reviews.Any(r => r.VenueId == venueId)
                        ? (decimal)reviews.Where(r => r.VenueId == venueId).Average(r => r.Rating)
                        : 0
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = reviewDtos,
                statistics = new
                {
                    totalReviews = reviews.Count,
                    averageRating = overallAverageRating,
                    venueStats = venueStats.Select(vs => new
                    {
                        venueId = vs.VenueId,
                        venueName = vs.VenueName,
                        reviewCount = vs.Reviews.Count,
                        averageRating = vs.AverageRating
                    })
                }
            });
        }

        // Helper method to calculate venue average rating
        private async Task<decimal> CalculateVenueAverageRating(int venueId)
        {
            var reviews = await _context.Tblreviews
                .Where(r => r.VenueId == venueId)
                .ToListAsync();

            // Add explicit cast to decimal
            return reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0m;
        }

        private bool TblreviewExists(int id)
        {
            return _context.Tblreviews.Any(e => e.ReviewId == id);
        }
    }
}
