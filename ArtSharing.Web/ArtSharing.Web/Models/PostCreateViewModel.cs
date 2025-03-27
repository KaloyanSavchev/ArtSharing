using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArtSharing.Web.Models
{
    public class PostCreateViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; }
    }
}
