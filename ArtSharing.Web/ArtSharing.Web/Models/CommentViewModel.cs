using System.ComponentModel.DataAnnotations;

namespace ArtSharing.Web.Models
{
    public class CommentViewModel
    {
        [Required]
        public string Content { get; set; }

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
