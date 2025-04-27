using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface IReportService
    {
        Task<bool> CheckIfPostAlreadyReportedAsync(string userId, int postId);
        Task<bool> CheckIfCommentAlreadyReportedAsync(string userId, int commentId);
        Task<bool> CheckIfUserAlreadyReportedAsync(string reporterId, string reportedUserId);

        Task ReportPostAsync(string userId, int postId, string reason);
        Task ReportCommentAsync(string userId, int commentId, string reason);
        Task ReportUserAsync(string reporterId, string reportedUserId, string reason);
    }
}
