using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly ILikeService _likeService;
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;

        public LikeController(ILikeService likeService, UserManager<ArtSharing.Data.Models.Models.User> userManager)
        {
            _likeService = likeService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike([FromBody] LikeDto data)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var (hasLiked, likeCount) = await _likeService.ToggleLikeAsync(data, user.Id);

            return Json(new { hasLiked, likeCount });
        }
    }
}
