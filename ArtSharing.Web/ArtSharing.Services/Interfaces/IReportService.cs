using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface IReportService
    {
        Task<bool> HasAlreadyReportedPostAsync(string userId, int postId);
        Task<bool> HasAlreadyReportedCommentAsync(string userId, int commentId);
        Task<bool> HasAlreadyReportedUserAsync(string userId, string targetUserId);

        Task ReportPostAsync(string userId, int postId, string reason);
        Task ReportCommentAsync(string userId, int commentId, string reason);
        Task ReportUserAsync(string reporterId, string targetUserId, string reason);
    }
}
