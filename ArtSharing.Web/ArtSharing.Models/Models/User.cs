using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Models
{
    public class User : IdentityUser    
    {
        public string? ProfilePicture { get; set; } 
   
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();

    }
}
