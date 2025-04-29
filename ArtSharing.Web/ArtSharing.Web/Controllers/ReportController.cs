using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ArtSharing.Data.Models.Models;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<User> _userManager;

        public ReportController(IReportService reportService, UserManager<User> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        public IActionResult Post(int id)
        {
            ViewBag.PostId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPostReport(int postId, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var alreadyReported = await _reportService.HasAlreadyReportedPostAsync(user.Id, postId);
            if (alreadyReported)
            {
                ViewBag.PostId = postId;
                return View("AlreadyReportedPost");
            }

            await _reportService.ReportPostAsync(user.Id, postId, reason);

            TempData["Message"] = "Post reported successfully.";
            return RedirectToAction("Details", "Post", new { id = postId });
        }

        public IActionResult Comment(int id)
        {
            ViewBag.CommentId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCommentReport(int commentId, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var alreadyReported = await _reportService.HasAlreadyReportedCommentAsync(user.Id, commentId);
            if (alreadyReported)
            {
                ViewBag.CommentId = commentId;
                return View("AlreadyReportedComment");
            }

            await _reportService.ReportCommentAsync(user.Id, commentId, reason);

            TempData["Message"] = "Comment reported successfully.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult UserProfile(string id)
        {
            ViewBag.TargetUserId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitUserReport(string targetUserId, string reason)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (currentUser == null || targetUser == null)
                return Unauthorized();

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            if (targetRoles.Contains("Admin"))
            {
                TempData["ErrorMessage"] = "You cannot report an Admin account.";
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            }

            var alreadyReported = await _reportService.HasAlreadyReportedUserAsync(currentUser.Id, targetUserId);
            if (alreadyReported)
            {
                ViewBag.TargetUserId = targetUserId;
                return View("AlreadyReported");
            }

            await _reportService.ReportUserAsync(currentUser.Id, targetUserId, reason);

            TempData["Message"] = "User reported successfully.";
            return RedirectToAction("About", "Profile", new { username = targetUser.UserName });
        }
    }
}
