using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly ApplicationDbContext _context;

        public AdminFeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Feedback>> GetAllFeedbacksAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return false;

            feedback.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return false;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearResolvedFeedbacksAsync()
        {
            var resolved = await _context.Feedbacks
                .Where(f => f.Status == "Resolved")
                .ToListAsync();

            if (!resolved.Any()) return false;

            _context.Feedbacks.RemoveRange(resolved);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
