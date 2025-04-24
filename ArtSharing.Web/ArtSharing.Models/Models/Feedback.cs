using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; } 

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved

    }

}
