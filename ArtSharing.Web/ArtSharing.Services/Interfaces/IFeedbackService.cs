using ArtSharing.ViewModels;

namespace ArtSharing.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task SubmitFeedbackAsync(FeedbackInputModel model, string userId);
    }
}
