using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    }
}
