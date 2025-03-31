using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public LikeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var existingLike = _context.Likes.FirstOrDefault(l => l.UserId == user.Id && l.PostId == postId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    UserId = user.Id,
                    PostId = postId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = postId });
        }
    }
}
