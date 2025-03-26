using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtSharing.Data.Models.Models;
using ArtSharing.Data.Models.Constants;

namespace ArtSharing.Data.Models.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(EntityConstants.MessageConstants.ContentMaxLength)]
        public string? Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("ChatRoom")]
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; }
    }
}
