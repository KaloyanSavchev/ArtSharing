using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtSharing.Web.Models.Dto;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike([FromBody] LikeDto data)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var post = await _context.Posts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == data.PostId);

            if (post == null)
                return NotFound();

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == user.Id);

            bool hasLiked;
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                hasLiked = false;
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    UserId = user.Id,
                    PostId = data.PostId,
                    CreatedAt = DateTime.UtcNow
                });
                hasLiked = true;
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.PostId == data.PostId);

            return Json(new { hasLiked, likeCount });
        }

    }

    public class LikeDto
    {
        public int PostId { get; set; }
    }
}
