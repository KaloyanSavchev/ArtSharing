using System.ComponentModel.DataAnnotations;

namespace ArtSharing.ViewModels
{
    public class FeedbackInputModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The subject must be less than 100 characters.")]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "The message must be less than 1000 characters.")]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
