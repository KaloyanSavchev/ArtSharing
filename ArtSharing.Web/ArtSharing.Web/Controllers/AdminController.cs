using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
            return RedirectToAction(nameof(ManageCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _adminService.DeleteCategoryAsync(id);
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
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveModeratorRole(string id)
        {
            await _adminService.RemoveModeratorRoleAsync(id);
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
            await _adminService.BanUserAsync(userId);
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUserFromList(string id)
        {
            await _adminService.BanUserFromListAsync(id);
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            await _adminService.UnbanUserAsync(id);
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            await _adminService.DeletePostAsync(postId);
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            await _adminService.DeleteCommentAsync(commentId);
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissReport(int reportId)
        {
            await _adminService.DismissReportAsync(reportId);
            return RedirectToAction(nameof(Reports));
        }
    }
}
