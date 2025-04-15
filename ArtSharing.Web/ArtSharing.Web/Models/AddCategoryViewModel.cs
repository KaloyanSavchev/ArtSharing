using System.ComponentModel.DataAnnotations;

namespace ArtSharing.Web.Models
{
    public class AddCategoryViewModel
    {
        [Required]
        [StringLength(25, ErrorMessage = "Name must be under 25 characters.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Description must be under 100 characters.")]
        public string Description { get; set; }
    }
}
