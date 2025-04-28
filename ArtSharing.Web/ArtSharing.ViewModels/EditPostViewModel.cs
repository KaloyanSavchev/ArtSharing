namespace ArtSharing.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
    }
}
