using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;

        public CommentController(ICommentService commentService, UserManager<ArtSharing.Data.Models.Models.User> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", "Post", new { id = postId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _commentService.CreateCommentAsync(postId, content, user.Id, parentCommentId);

            return RedirectToAction("Details", "Post", new { id = postId });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return Forbid();

            return View(comment);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Content cannot be empty.";
                return RedirectToAction("Edit", new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return Forbid();

            var success = await _commentService.UpdateCommentAsync(id, content, user.Id);
            if (!success) return Forbid();

            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }


        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return Forbid();

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();

            var postId = comment.PostId; 

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return Forbid();

            var success = await _commentService.DeleteCommentAsync(id, user.Id);
            if (!success) return Forbid();

            return RedirectToAction("Details", "Post", new { id = postId });
        }
    }
}
