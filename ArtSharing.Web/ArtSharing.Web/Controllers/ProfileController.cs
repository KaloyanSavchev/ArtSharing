using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
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
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<User> userManager, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        public async Task<IActionResult> Index(string tab = "MyPosts")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var followersCount = await _context.UserFollows.CountAsync(f => f.FollowingId == user.Id);
            var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == user.Id);

            var myPosts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Category)
                .ToListAsync();

            var likedPosts = await _context.Likes
                .Where(l => l.UserId == user.Id)
                .Include(l => l.Post)
                    .ThenInclude(p => p.User)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Category)
                .Select(l => l.Post)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture,
                ProfileDescription = user.ProfileDescription,
                DateRegistered = user.DateRegistered,
                UserId = user.Id,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsOwnProfile = true,
                SelectedTab = tab,
                UserPosts = myPosts,
                LikedPosts = likedPosts
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model, IFormFile profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "profile_pics");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                user.ProfilePicture = "/profile_pics/" + uniqueFileName;
            }

            user.ProfileDescription = model.ProfileDescription;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);

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
                ProfilePicture = user.ProfilePicture,
                ProfileDescription = user.ProfileDescription
            };

            return View(model);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.ProfileDescription = model.ProfileDescription;

            if (model.ProfileImageFile != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
               
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImageFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(stream);
                }

                user.ProfilePicture = "/images/profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return RedirectToAction("Index");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> About(string username)
        {
            if (string.IsNullOrEmpty(username))
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwnProfile = currentUser != null && currentUser.Id == user.Id;

            bool isFollowing = false;
            if (currentUser != null && !isOwnProfile)
            {
                isFollowing = await _context.UserFollows
                    .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == user.Id);
            }

            var userPosts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Category)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture,
                ProfileDescription = user.ProfileDescription,
                DateRegistered = user.DateRegistered,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                IsFollowing = isFollowing,
                IsOwnProfile = isOwnProfile,
                UserPosts = userPosts
            };

            return View("About", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (targetUser == null || currentUser == null || currentUser.Id == targetUser.Id)
                return BadRequest();

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == targetUser.Id);

            bool isFollowing;

            if (existingFollow != null)
            {
                _context.UserFollows.Remove(existingFollow);
                isFollowing = false;
            }
            else
            {
                _context.UserFollows.Add(new UserFollow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = targetUser.Id
                });
                isFollowing = true;
            }

            await _context.SaveChangesAsync();

            var followersCount = await _context.UserFollows.CountAsync(f => f.FollowingId == targetUser.Id);
            var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == targetUser.Id);

            return Json(new
            {
                isFollowing,
                followersCount,
                followingCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> LoadFollowList(string type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            List<User> users;

            if (type == "followers")
            {
                users = await _context.UserFollows
                    .Where(f => f.FollowingId == user.Id)
                    .Select(f => f.Follower)
                    .ToListAsync();
            }
            else if (type == "following")
            {
                users = await _context.UserFollows
                    .Where(f => f.FollowerId == user.Id)
                    .Select(f => f.Following)
                    .ToListAsync();
            }
            else return BadRequest();

            return PartialView("_FollowListPartial", users);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowList(string type)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            List<User> users = new();
            if (type == "followers")
            {
                users = await _context.UserFollows
                    .Where(f => f.FollowingId == currentUser.Id)
                    .Select(f => f.Follower)
                    .ToListAsync();
            }
            else if (type == "following")
            {
                users = await _context.UserFollows
                    .Where(f => f.FollowerId == currentUser.Id)
                    .Select(f => f.Following)
                    .ToListAsync();
            }

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
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Category)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync(u => u.UserName == username);

            var currentUser = await _userManager.GetUserAsync(User);

            bool isFollowing = await _context.UserFollows.AnyAsync(f =>
                f.FollowerId == currentUser.Id && f.FollowingId == user.Id);

            if (user == null || (!isFollowing && user.Id != currentUser.Id))
                return Unauthorized();

            return PartialView("_UserPostsPartial", user.Posts.ToList());
        }
    }
}
