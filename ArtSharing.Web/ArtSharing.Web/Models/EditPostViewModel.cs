using ArtSharing.Data.Models.Constants;
using System.ComponentModel.DataAnnotations;
namespace ArtSharing.Web.Models
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(EntityConstants.PostConstants.TitleMaxLength)]
        public string Title { get; set; }

        [MaxLength(EntityConstants.PostConstants.DescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
