using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommentService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateCommentAsync(int postId, string content, string userId, int? parentCommentId)
        {
            var comment = new Comment
            {
                Content = content,
                PostId = postId,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Replies)
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateCommentAsync(int id, string newContent, string userId)
        {
            var comment = await _context.Comments.FindAsync(id);
            var user = await _userManager.FindByIdAsync(userId);

            if (comment == null || user == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return false;

            comment.Content = newContent;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (comment.UserId != user.Id && !isAdmin && !isModerator)
                return false;

            if (comment.Replies != null && comment.Replies.Any())
            {
                _context.Comments.RemoveRange(comment.Replies);
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
