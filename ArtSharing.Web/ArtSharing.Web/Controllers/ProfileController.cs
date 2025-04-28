using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using ArtSharing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;
        private readonly IProfileService _profileService;

        public ProfileController(UserManager<ArtSharing.Data.Models.Models.User> userManager, IProfileService profileService)
        {
            _userManager = userManager;
            _profileService = profileService;
        }

        public async Task<IActionResult> Index(string tab = "MyPosts")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var model = await _profileService.GetOwnProfileAsync(user, tab);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            await _profileService.UpdateProfileAsync(user, new EditProfileViewModel
            {
                PhoneNumber = model.PhoneNumber,
                ProfileDescription = model.ProfileDescription
            }, profileImage);

            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileDescription = user.ProfileDescription,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            await _profileService.UpdateProfileAsync(user, model, model.ProfileImageFile);

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> About(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var model = await _profileService.GetUserProfileAsync(username, currentUser);
            if (model == null) return NotFound();

            return View("About", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (currentUser == null || targetUser == null)
                return BadRequest();

            var isFollowing = await _profileService.ToggleFollowAsync(currentUser, targetUser);

            var followersCount = await _userManager.Users
                .Where(u => u.Id == targetUser.Id)
                .Select(u => u.Followers.Count)
                .FirstOrDefaultAsync();

            var followingCount = await _userManager.Users
                .Where(u => u.Id == targetUser.Id)
                .Select(u => u.Following.Count)
                .FirstOrDefaultAsync();

            return Json(new
            {
                isFollowing,
                followersCount,
                followingCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowList(string type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var users = await _profileService.GetFollowListAsync(user, type);

            var result = users.Select(u => new
            {
                u.UserName,
                ProfilePicture = string.IsNullOrEmpty(u.ProfilePicture) ? "/images/default-profile.png" : u.ProfilePicture
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPostsPartial(string username)
        {
            var user = await _userManager.Users
                .Include(u => u.Posts).ThenInclude(p => p.Category)
                .Include(u => u.Posts).ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return NotFound();

            return PartialView("_UserPostsPartial", user.Posts.ToList());
        }
    }
}
