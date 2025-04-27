using ArtSharing.Data.Models.Models;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ArtSharing.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetOwnProfileAsync(User user, string selectedTab);
        Task<ProfileViewModel> GetUserProfileAsync(string username, User? currentUser);
        Task UpdateProfileAsync(User user, EditProfileViewModel model, IFormFile? profileImage);
        Task<bool> ToggleFollowAsync(User currentUser, User targetUser);
        Task<List<User>> GetFollowListAsync(User currentUser, string type);
        Task<List<Post>> GetUserPostsAsync(string username, string currentUserId);
    }
}
