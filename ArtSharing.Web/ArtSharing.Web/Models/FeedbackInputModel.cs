using System.ComponentModel.DataAnnotations;

namespace ArtSharing.Web.Models
{
    public class FeedbackInputModel
    {
        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
