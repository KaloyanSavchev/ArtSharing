using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
