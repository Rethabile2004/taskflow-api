using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(IgnoreApi =true)]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SeedController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("categories")]
        public async Task<IActionResult> SeedCategories()
        {
            if (!_env.IsDevelopment())
                return NotFound();
            // Guard: if any categories exist, do not seed
            bool hasData = await _context.Categories.AnyAsync();
            if (hasData)
                return Conflict(new { message = "Categories table already has data. Seeding skipped." });

            var categories = new List<Category>
            {
                new Category { Name = "Work" },
                new Category { Name = "Personal" },
                new Category { Name = "Health & Fitness" },
                new Category { Name = "Finance" },
                new Category { Name = "Learning" }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{categories.Count} categories seeded successfully." });
        }
    }
}