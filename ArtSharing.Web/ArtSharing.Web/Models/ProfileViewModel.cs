using ArtSharing.Data.Models.Models;

namespace ArtSharing.Web.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public string? ProfileDescription { get; set; }

        public DateTime DateRegistered { get; set; }

        public string UserId { get; set; } 

        public bool IsFollowing { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public bool IsOwnProfile { get; set; }

        public List<Post> UserPosts { get; set; } = new();

        public List<Post> LikedPosts { get; set; } = new();

        public string SelectedTab { get; set; } = "MyPosts";
    }
}
