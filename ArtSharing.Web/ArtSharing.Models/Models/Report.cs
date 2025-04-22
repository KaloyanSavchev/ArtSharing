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
        public int Id { get; set; }

        [Required]
        public string ReporterId { get; set; }
        [ForeignKey(nameof(ReporterId))]
        public User Reporter { get; set; }

        [Required]
        public string TargetType { get; set; }

        public int? TargetPostId { get; set; }
        public Post? TargetPost { get; set; }

        public int? TargetCommentId { get; set; }
        public Comment? TargetComment { get; set; }

        public string? TargetUserId { get; set; }
        public User? TargetUser { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; 
    }
}
