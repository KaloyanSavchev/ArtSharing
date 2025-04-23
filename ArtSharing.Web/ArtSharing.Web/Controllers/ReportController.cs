using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
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
           
            var existing = _context.Reports.FirstOrDefault(r =>
                r.ReporterId == user.Id &&
                r.TargetPostId == postId &&
                r.TargetType == "Post");

            if (existing != null)
            {
                ViewBag.PostId = postId;
                return View("AlreadyReportedPost");
            }

            var report = new Report
            {
                ReporterId = user.Id,
                TargetPostId = postId,
                TargetType = "Post",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

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

            var existing = _context.Reports.FirstOrDefault(r =>
                r.ReporterId == user.Id &&
                r.TargetCommentId == commentId &&
                r.TargetType == "Comment");

            if (existing != null)
            {
                return View("AlreadyReportedComment");
            }

            var report = new Report
            {
                ReporterId = user.Id,
                TargetCommentId = commentId,
                TargetType = "Comment",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

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
            if (currentUser == null || string.IsNullOrEmpty(targetUserId))
                return Unauthorized();

            var reportedUser = await _userManager.FindByIdAsync(targetUserId);
            if (reportedUser == null)
                return NotFound();

            var existing = _context.Reports.FirstOrDefault(r =>
                r.ReporterId == currentUser.Id &&
                r.TargetUserId == targetUserId &&
                r.TargetType == "User");

            if (existing != null)
            {
                ViewBag.UserName = reportedUser.UserName;
                return View("AlreadyReported");
            }
            
            var report = new Report
            {
                ReporterId = currentUser.Id,
                TargetUserId = targetUserId,
                TargetType = "User",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Message"] = "User reported successfully.";
            return RedirectToAction("About", "Profile", new { username = reportedUser.UserName });
        }
    }
}
