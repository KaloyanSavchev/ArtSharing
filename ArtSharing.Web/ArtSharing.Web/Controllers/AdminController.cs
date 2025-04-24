using ArtSharing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    using ArtSharing.Data.Models.Models;
    using ArtSharing.Web.Models;
    using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> ManageUsers()
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
                    IsBanned = user.IsBanned,
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

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveModeratorRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Moderator");
            }

            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.TargetUser)
                .Include(r => r.TargetPost).ThenInclude(p => p.User)
                .Include(r => r.TargetComment).ThenInclude(c => c.User)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsBanned = true; 
            await _userManager.UpdateAsync(user);

            
            var relatedReports = _context.Reports.Where(r => r.TargetUserId == userId);
            _context.Reports.RemoveRange(relatedReports);

            await _context.SaveChangesAsync();
            return RedirectToAction("Reports");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUserFromList(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = false;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);

            var relatedReports = _context.Reports.Where(r => r.TargetPostId == postId);
            _context.Reports.RemoveRange(relatedReports);

            await _context.SaveChangesAsync();
            return RedirectToAction("Reports");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);

            var relatedReports = _context.Reports.Where(r => r.TargetCommentId == commentId);
            _context.Reports.RemoveRange(relatedReports);

            await _context.SaveChangesAsync();
            return RedirectToAction("Reports");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissReport(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return NotFound();

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("Reports");
        }

    }
}
