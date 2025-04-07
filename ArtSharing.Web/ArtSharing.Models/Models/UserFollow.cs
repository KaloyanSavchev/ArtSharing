using System.ComponentModel.DataAnnotations;

namespace ArtSharing.Data.Models.Models
{
    public class UserFollow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; }  // Followers
        public User Follower { get; set; }

        [Required]
        public string FollowingId { get; set; } // Following 
        public User Following { get; set; }
    }
}
