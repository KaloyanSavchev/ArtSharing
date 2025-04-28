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

        public async Task<List<Post>> GetInitialPostsAsync(int take = 6, string sortOrder = "newest")
        {
            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            postsQuery = ApplySorting(postsQuery, sortOrder);

            return await postsQuery.Take(take).ToListAsync();
        }

        public async Task<List<Post>> LoadMorePostsAsync(int skip = 0, int take = 6, string sortOrder = "newest")
        {
            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            postsQuery = ApplySorting(postsQuery, sortOrder);

            return await postsQuery.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<List<Post>> FilterPostsAsync(string? searchTerm, int? categoryId, string sortOrder = "newest")
        {
            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.CategoryId == categoryId);
            }

            postsQuery = ApplySorting(postsQuery, sortOrder);

            return await postsQuery.ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        private IQueryable<Post> ApplySorting(IQueryable<Post> query, string sortOrder)
        {
            return sortOrder switch
            {
                "popular" => query.OrderByDescending(p => p.Likes.Count),
                "leastliked" => query.OrderBy(p => p.Likes.Count),
                "oldest" => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt),
            };
        }
    }
}
