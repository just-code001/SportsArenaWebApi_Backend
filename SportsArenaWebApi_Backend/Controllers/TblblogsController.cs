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
    public class TblblogsController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblblogsController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblblogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetTblblogs()
        {
            var blogs = await _context.Tblblogs
                .Include(b => b.Author)
                .OrderByDescending(b => b.PublishDate)
                .Select(b => new BlogResponseDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Content = b.Content,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    PublishDate = b.PublishDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { success = true, data = blogs });
        }

        // GET: api/Tblblogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogResponseDto>> GetTblblog(int id)
        {
            var blog = await _context.Tblblogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
            {
                return NotFound(new { success = false, message = "Blog not found." });
            }

            var blogDto = new BlogResponseDto
            {
                BlogId = blog.BlogId,
                Title = blog.Title,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                AuthorName = blog.Author.Name,
                PublishDate = blog.PublishDate ?? DateTime.UtcNow
            };

            return Ok(new { success = true, data = blogDto });
        }

        // GET: api/Tblblogs/author/5
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetBlogsByAuthor(int authorId)
        {
            var author = await _context.Tblusers.FindAsync(authorId);
            if (author == null)
            {
                return NotFound(new { success = false, message = "Author not found." });
            }

            var blogs = await _context.Tblblogs
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .OrderByDescending(b => b.PublishDate)
                .Select(b => new BlogResponseDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Content = b.Content,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    PublishDate = b.PublishDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { success = true, data = blogs });
        }

        // GET: api/Tblblogs/my-blogs
        [HttpGet("my-blogs")]
        [Authorize(Roles = "admin,provider")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> GetMyBlogs()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var blogs = await _context.Tblblogs
                .Include(b => b.Author)
                .Where(b => b.AuthorId == userId)
                .OrderByDescending(b => b.PublishDate)
                .Select(b => new BlogResponseDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Content = b.Content,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    PublishDate = b.PublishDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { success = true, data = blogs });
        }

        // POST: api/Tblblogs/search
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<BlogResponseDto>>> SearchBlogs(SearchBlogDto searchDto)
        {
            var query = _context.Tblblogs
                .Include(b => b.Author)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) ||
                                        b.Content.ToLower().Contains(searchTerm));
            }

            if (searchDto.AuthorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == searchDto.AuthorId.Value);
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(b => b.PublishDate >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(b => b.PublishDate <= searchDto.ToDate.Value);
            }

            // Calculate pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            // Apply pagination
            var blogs = await query
                .OrderByDescending(b => b.PublishDate)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(b => new BlogResponseDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Content = b.Content,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    PublishDate = b.PublishDate ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = blogs,
                pagination = new
                {
                    currentPage = searchDto.Page,
                    pageSize = searchDto.PageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                }
            });
        }

        // POST: api/Tblblogs
        [HttpPost]
        [Authorize(Roles = "admin,provider")]
        public async Task<ActionResult<BlogResponseDto>> CreateBlog(CreateBlogDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value!);

            var user = await _context.Tblusers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var blog = new Tblblog
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorId = userId,
                PublishDate = DateTime.UtcNow
            };

            _context.Tblblogs.Add(blog);
            await _context.SaveChangesAsync();

            var blogDto = new BlogResponseDto
            {
                BlogId = blog.BlogId,
                Title = blog.Title,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                AuthorName = user.Name,
                PublishDate = blog.PublishDate ?? DateTime.UtcNow
            };

            return CreatedAtAction("GetTblblog", new { id = blog.BlogId }, new { success = true, data = blogDto });
        }

        // PUT: api/Tblblogs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,provider")]
        public async Task<IActionResult> UpdateBlog(int id, UpdateBlogDto dto)
        {
            var blog = await _context.Tblblogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound(new { success = false, message = "Blog not found." });
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value!);
            var userRole = User.FindFirst("Role")?.Value;

            // Check if the user is the author of the blog or an admin
            if (blog.AuthorId != userId && userRole != "admin")
            {
                return Forbid();
            }

            blog.Title = dto.Title;
            blog.Content = dto.Content;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Blog updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblblogExists(id))
                {
                    return NotFound(new { success = false, message = "Blog not found." });
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Tblblogs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,provider")]
        public async Task<IActionResult> DeleteTblblog(int id)
        {
            var blog = await _context.Tblblogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound(new { success = false, message = "Blog not found." });
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value!);
            var userRole = User.FindFirst("Role")?.Value;

            // Check if the user is the author of the blog or an admin
            if (blog.AuthorId != userId && userRole != "admin")
            {
                return Forbid();
            }

            _context.Tblblogs.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Blog deleted successfully." });
        }

        private bool TblblogExists(int id)
        {
            return _context.Tblblogs.Any(e => e.BlogId == id);
        }
    }
}
