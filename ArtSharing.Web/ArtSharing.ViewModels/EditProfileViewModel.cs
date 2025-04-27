using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArtSharing.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? ProfileDescription { get; set; }

        public string? ProfilePicture { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}
