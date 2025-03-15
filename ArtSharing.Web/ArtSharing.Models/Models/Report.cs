using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; }

        [ForeignKey("ReportedBy")]
        public string ReportedById { get; set; }
        public User ReportedBy { get; set; }

        public int? PostId { get; set; }
        public Post Post { get; set; }

        public int? CommentId { get; set; }
        public Comment Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
