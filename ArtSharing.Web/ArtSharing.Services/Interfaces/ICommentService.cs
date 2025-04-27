using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface ICommentService
    {
        Task CreateCommentAsync(int postId, string content, string userId, int? parentCommentId);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<bool> UpdateCommentAsync(int id, string newContent, string userId);
        Task<bool> DeleteCommentAsync(int id, string userId);
    }
}
