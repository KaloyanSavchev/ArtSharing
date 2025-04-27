using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class PostHomeService : IPostHomeService
    {
        private readonly ApplicationDbContext _context;

        public PostHomeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetInitialPostsAsync(int take = 6)
        {
            return await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Post>> LoadMorePostsAsync(int skip = 0, int take = 6)
        {
            return await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(p => p.User)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<List<Post>> FilterPostsAsync(string? searchTerm, int? categoryId)
        {
            return await _context.Posts
                .Where(p =>
                    (string.IsNullOrEmpty(searchTerm) ||
                     p.Title.Contains(searchTerm) ||
                     p.Description.Contains(searchTerm)) &&
                    (!categoryId.HasValue || p.CategoryId == categoryId.Value))
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
    }
}
