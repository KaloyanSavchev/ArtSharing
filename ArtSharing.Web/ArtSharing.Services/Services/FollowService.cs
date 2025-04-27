using ArtSharing.Data;
using ArtSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
        {
            if (followerId == followingId) return false;

            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
            {
                _context.UserFollows.Remove(existingFollow);
            }
            else
            {
                var follow = new Data.Models.Models.UserFollow
                {
                    FollowerId = followerId,
                    FollowingId = followingId
                };
                _context.UserFollows.Add(follow);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
