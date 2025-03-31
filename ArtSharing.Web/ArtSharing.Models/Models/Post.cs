using ArtSharing.Data.Models.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtSharing.Data.Models.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(EntityConstants.PostConstants.TitleMaxLength)]
        public string Title { get; set; }

        [MaxLength(EntityConstants.PostConstants.DescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        public string ImagePath { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; }
        [BindNever]
        public User? User { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [BindNever]
        public Category? Category { get; set; }

        public List<SavedPost> SavedByUsers { get; set; } = new List<SavedPost>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
