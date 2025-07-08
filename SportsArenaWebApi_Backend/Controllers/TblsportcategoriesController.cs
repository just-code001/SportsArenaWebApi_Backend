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
    [Authorize(Roles = "admin")]
    public class TblsportcategoriesController : ControllerBase
    {
        private readonly SportsArenaDbContext _context;

        public TblsportcategoriesController(SportsArenaDbContext context)
        {
            _context = context;
        }

        // GET: api/Tblsportcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tblsportcategory>>> GetTblsportcategories()
        {
            var categories = await _context.Tblsportcategories.ToListAsync();

            return Ok(new
            {
                success = true,
                message = "all categories recieved successfully.",
                data = categories
            });
        }

        // GET: api/Tblsportcategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tblsportcategory>> GetTblsportcategory(int id)
        {
            var tblsportcategory = await _context.Tblsportcategories.FindAsync(id);

            if (tblsportcategory == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "No category Found.",
                });
            }

            return Ok(new
            {
                success = true,
                message = "category found successfully.",
                data = tblsportcategory
            });
        }

        // PUT: api/Tblsportcategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTblsportcategory(int id, [FromBody] SportsCategoryDto sportsCategoryDto)
        {

            var category = await _context.Tblsportcategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No Category Found."
                });
            }

            category.Categoryname = sportsCategoryDto.CategoryName;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Sport category updated successfully.",
                    data = category
                });
        }

        // POST: api/Tblsportcategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tblsportcategory>> PostTblsportcategory([FromBody] SportsCategoryDto sportsCategoryDto)
        {
            if (await _context.Tblsportcategories.AnyAsync(s => s.Categoryname.ToLower() == sportsCategoryDto.CategoryName.ToLower()))
            {
                return BadRequest(new { 
                    success = false,
                    message = "category already exist"
                });
            }

            var category = new Tblsportcategory
            {
                Categoryname = sportsCategoryDto.CategoryName
            };

            _context.Tblsportcategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                message = "New Category Added.",
                data = category
            });
        }

        // DELETE: api/Tblsportcategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTblsportcategory(int id)
        {
            var tblsportcategory = await _context.Tblsportcategories.FindAsync(id);
            if (tblsportcategory == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No Category Found."
                });
            }

            _context.Tblsportcategories.Remove(tblsportcategory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Sport category deleted successfully."
            });
        }

        private bool TblsportcategoryExists(int id)
        {
            return _context.Tblsportcategories.Any(e => e.CategoryId == id);
        }
    }
}
