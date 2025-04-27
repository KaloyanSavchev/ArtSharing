using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckIfPostAlreadyReportedAsync(string userId, int postId)
        {
            return await _context.Reports
                .AnyAsync(r => r.ReporterId == userId && r.TargetPostId == postId && r.TargetType == "Post");
        }

        public async Task<bool> CheckIfCommentAlreadyReportedAsync(string userId, int commentId)
        {
            return await _context.Reports
                .AnyAsync(r => r.ReporterId == userId && r.TargetCommentId == commentId && r.TargetType == "Comment");
        }

        public async Task<bool> CheckIfUserAlreadyReportedAsync(string reporterId, string reportedUserId)
        {
            return await _context.Reports
                .AnyAsync(r => r.ReporterId == reporterId && r.TargetUserId == reportedUserId && r.TargetType == "User");
        }

        public async Task ReportPostAsync(string userId, int postId, string reason)
        {
            var report = new Report
            {
                ReporterId = userId,
                TargetPostId = postId,
                TargetType = "Post",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task ReportCommentAsync(string userId, int commentId, string reason)
        {
            var report = new Report
            {
                ReporterId = userId,
                TargetCommentId = commentId,
                TargetType = "Comment",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task ReportUserAsync(string reporterId, string reportedUserId, string reason)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                TargetUserId = reportedUserId,
                TargetType = "User",
                Reason = reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }
    }
}
