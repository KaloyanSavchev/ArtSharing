using ArtSharing.Data.Models.Constants;
using System.ComponentModel.DataAnnotations;
namespace ArtSharing.Web.Models
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public string ImagePath { get; set; }
    }

}
