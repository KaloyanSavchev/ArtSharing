using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtSharing.Data.Models.Constants;

namespace ArtSharing.Data.Models.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(EntityConstants.CommentConstants.ContentMaxLength)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

        public int? ParentCommentId { get; set; } 
        public Comment? ParentComment { get; set; }

        public List<Comment> Replies { get; set; } = new();
    }   
}
