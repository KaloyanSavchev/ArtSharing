using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface IPostHomeService
    {
        Task<List<Post>> GetInitialPostsAsync(int take = 6, string sortOrder = "newest");
        Task<List<Post>> LoadMorePostsAsync(int skip = 0, int take = 6, string sortOrder = "newest");
        Task<List<Post>> FilterPostsAsync(string? searchTerm, int? categoryId, string sortOrder = "newest");
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
