using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArtSharing.ViewModels
{
    public class PostCreateViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "The title must be between 3 and 100 characters.", MinimumLength = 3)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "The description must be at most 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "You must upload an image.")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
    }
}
