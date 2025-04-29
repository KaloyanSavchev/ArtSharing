using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<User> _userManager;


        public AdminController(IAdminService adminService, UserManager<User> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }


        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _adminService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> CreateCategory(string name, string description)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _adminService.CreateCategoryAsync(name, description);
                TempData["SuccessMessage"] = "Category created successfully.";
            }

            return RedirectToAction(nameof(ManageCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _adminService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> EditCategory(int id, Category updatedCategory)
        {
            if (id != updatedCategory.Id) return BadRequest();

            await _adminService.EditCategoryAsync(updatedCategory);
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(ManageCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _adminService.DeleteCategoryAsync(id);
            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(ManageCategories));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _adminService.GetAllUsersWithRolesAsync();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PromoteToModerator(string id)
        {
            await _adminService.PromoteToModeratorAsync(id);
            TempData["SuccessMessage"] = "User promoted to moderator.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveModeratorRole(string id)
        {
            await _adminService.RemoveModeratorRoleAsync(id);
            TempData["SuccessMessage"] = "Moderator role removed successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Reports()
        {
            var reports = await _adminService.GetAllReportsAsync();
            return View(reports);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                TempData["ErrorMessage"] = "You cannot ban an Admin account.";
                return RedirectToAction(nameof(Reports));
            }

            await _adminService.BanUserAsync(userId);
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUserFromList(string id)
        {
            await _adminService.BanUserFromListAsync(id);
            TempData["SuccessMessage"] = "User banned successfully from the list.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            await _adminService.UnbanUserAsync(id);
            TempData["SuccessMessage"] = "User unbanned successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            await _adminService.DeletePostAsync(postId);
            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            await _adminService.DeleteCommentAsync(commentId);
            TempData["SuccessMessage"] = "Comment deleted successfully.";
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissReport(int reportId)
        {
            await _adminService.DismissReportAsync(reportId);
            TempData["SuccessMessage"] = "Report dismissed successfully.";
            return RedirectToAction(nameof(Reports));
        }
    }
}
