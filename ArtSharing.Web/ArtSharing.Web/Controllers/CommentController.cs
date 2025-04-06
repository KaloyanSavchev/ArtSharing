using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int PostId, string Content, int? ParentCommentId)
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", "Post", new { id = PostId });
            }

            var user = await _userManager.GetUserAsync(User);

            var comment = new Comment
            {
                Content = Content,
                PostId = PostId,
                ParentCommentId = ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { id = PostId });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (comment.UserId != user.Id && !isAdmin) return Forbid();

            return View(comment);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comment updatedComment)
        {
            if (id != updatedComment.Id) return NotFound();

            var comment = await _context.Comments.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (comment == null || (comment.UserId != user.Id && !isAdmin)) return Forbid();

            comment.Content = updatedComment.Content;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (comment.UserId != user.Id && !isAdmin) return Forbid();

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (comment.UserId != user.Id && !isAdmin) return Forbid();

            // Изтриване на всички отговори
            _context.Comments.RemoveRange(comment.Replies);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

    }
}
