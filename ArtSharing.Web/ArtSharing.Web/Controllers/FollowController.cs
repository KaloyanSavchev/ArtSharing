using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FollowController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id == userId)
                return BadRequest(); // Не може да следва себе си или не е логнат

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userId);

            if (existingFollow != null)
            {
                _context.UserFollows.Remove(existingFollow);
            }
            else
            {
                var follow = new UserFollow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId
                };
                _context.UserFollows.Add(follow);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("About", "Profile", new { id = userId });
        }
    }
}
