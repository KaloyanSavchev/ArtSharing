using ArtSharing.Data.Models.Models;
using ArtSharing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArtSharing.Web.Models;

namespace ArtSharing.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ManageCategories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name, string description)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var category = new Category
                {
                    Name = name,
                    Description = description
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageCategories");
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category updatedCategory)
        {
            if (id != updatedCategory.Id) return BadRequest();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCategories));
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageCategories));
        }
    }
}
