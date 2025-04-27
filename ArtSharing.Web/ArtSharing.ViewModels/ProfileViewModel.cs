using ArtSharing.Data.Models.Models;

namespace ArtSharing.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public string? ProfileDescription { get; set; }

        public DateTime DateRegistered { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public bool IsOwnProfile { get; set; }

        public bool IsFollowing { get; set; }

        public bool IsBanned { get; set; }

        public List<Post> UserPosts { get; set; } = new();

        public List<Post> LikedPosts { get; set; } = new();

        public string SelectedTab { get; set; } = "MyPosts";
    }
}
