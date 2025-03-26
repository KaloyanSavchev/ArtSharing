using ArtSharing.Data.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Models
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(EntityConstants.ChatRoomConstants.NameMaxLength)]
        public string? Name { get; set; }

        [Required, MaxLength(EntityConstants.ChatRoomConstants.DescriptionMaxLength)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
