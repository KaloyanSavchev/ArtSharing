using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Models
{
    public class SavedPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int PostId { get; set; }
        public Post Post { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }

}
