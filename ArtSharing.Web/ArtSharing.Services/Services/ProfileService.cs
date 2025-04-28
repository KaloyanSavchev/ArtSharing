using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileService(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<ProfileViewModel> GetOwnProfileAsync(User user, string selectedTab)
        {
            var followersCount = await _context.UserFollows.CountAsync(f => f.FollowingId == user.Id);
            var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == user.Id);

            var myPosts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Category)
                .Include(p => p.PostImages)
                .ToListAsync();

            var likedPosts = await _context.Likes
                .Where(l => l.UserId == user.Id)
                .Include(l => l.Post)
                    .ThenInclude(p => p.User)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Category)
                .Include(l => l.Post)
                    .ThenInclude(p => p.PostImages) 
                .Select(l => l.Post)
                .ToListAsync();

            return new ProfileViewModel
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
                SelectedTab = selectedTab,
                IsOwnProfile = true,
                UserPosts = myPosts,
                LikedPosts = likedPosts
            };
        }

        public async Task<ProfileViewModel> GetUserProfileAsync(string username, User? currentUser)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null) return null;

            bool isOwnProfile = currentUser != null && user.Id == currentUser.Id;
            bool isFollowing = false;

            if (!isOwnProfile && currentUser != null)
            {
                isFollowing = await _context.UserFollows
                    .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == user.Id);
            }

            var posts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Category)
                .Include(p => p.PostImages)
                .ToListAsync();

            return new ProfileViewModel
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
                UserId = user.Id,
                IsBanned = user.IsBanned,
                UserPosts = posts
            };
        }


        public async Task UpdateProfileAsync(User user, EditProfileViewModel model, IFormFile? profileImage)
        {
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.ProfileDescription = model.ProfileDescription;

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "images/profiles");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfilePicture = "/images/profiles/" + fileName;
            }

            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> ToggleFollowAsync(User currentUser, User targetUser)
        {
            if (currentUser == null || targetUser == null || currentUser.Id == targetUser.Id)
                return false;

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == targetUser.Id);

            if (existingFollow != null)
            {
                _context.UserFollows.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                _context.UserFollows.Add(new UserFollow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = targetUser.Id
                });
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<User>> GetFollowListAsync(User currentUser, string type)
        {
            if (type == "followers")
            {
                return await _context.UserFollows
                    .Where(f => f.FollowingId == currentUser.Id)
                    .Select(f => f.Follower)
                    .ToListAsync();
            }
            else if (type == "following")
            {
                return await _context.UserFollows
                    .Where(f => f.FollowerId == currentUser.Id)
                    .Select(f => f.Following)
                    .ToListAsync();
            }
            return new List<User>();
        }

        public async Task<List<Post>> GetUserPostsAsync(string username, string currentUserId)
        {
            var user = await _context.Users
                .Include(u => u.Posts).ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null) return new List<Post>();

            bool isFollowing = await _context.UserFollows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);

            if (user.Id != currentUserId && !isFollowing)
                return new List<Post>();

            return user.Posts.ToList();
        }
    }
}
