using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitFeedbackAsync(FeedbackInputModel model, string userId)
        {
            var feedback = new Feedback
            {
                Subject = model.Subject,
                Message = model.Message,
                Type = model.Type,
                UserId = userId,
                Status = "Open",
                SubmittedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
