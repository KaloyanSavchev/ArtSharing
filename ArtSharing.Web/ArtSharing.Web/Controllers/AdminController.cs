using ArtSharing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    using ArtSharing.Data.Models.Models;
    using ArtSharing.Web.Models;

    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;


        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ManageCategories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
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

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
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

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ManageModerators()
        {
            var users = _userManager.Users.ToList();

            var model = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PromoteToModerator(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Проверка дали вече е модератор или админ
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Moderator") && !roles.Contains("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Moderator");
            }

            return RedirectToAction("ManageModerators");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveModerator(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Moderator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Moderator");
            }

            return RedirectToAction("ManageModerators");
        }
    }
}
