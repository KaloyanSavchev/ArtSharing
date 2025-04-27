using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface IAdminFeedbackService
    {
        Task<List<Feedback>> GetAllFeedbacksAsync();
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteFeedbackAsync(int id);
        Task<bool> ClearResolvedFeedbacksAsync();
    }
}
