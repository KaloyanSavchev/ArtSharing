using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtSharing.Data.Models.Models
{
    public class PostImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = null!;

        [ForeignKey(nameof(Post))]
        public int PostId { get; set; }

        public Post Post { get; set; } = null!;
    }

}
