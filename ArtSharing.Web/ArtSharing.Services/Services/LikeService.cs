using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool hasLiked, int likeCount)> ToggleLikeAsync(LikeDto data, string userId)
        {
            var post = await _context.Posts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == data.PostId);

            if (post == null)
            {
                return (false, 0);
            }

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);
            bool hasLiked;

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                hasLiked = false;
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    UserId = userId,
                    PostId = data.PostId,
                    CreatedAt = DateTime.UtcNow
                });
                hasLiked = true;
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.PostId == data.PostId);

            return (hasLiked, likeCount);
        }
    }
}
