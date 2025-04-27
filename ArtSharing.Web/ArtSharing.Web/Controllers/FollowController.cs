using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IFollowService _followService;
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;

        public FollowController(IFollowService followService, UserManager<ArtSharing.Data.Models.Models.User> userManager)
        {
            _followService = followService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var success = await _followService.ToggleFollowAsync(currentUser.Id, userId);
            if (!success) return BadRequest();

            return RedirectToAction("About", "Profile", new { id = userId });
        }
    }
}
