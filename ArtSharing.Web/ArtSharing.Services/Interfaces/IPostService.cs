using ArtSharing.Data.Models.Models;
using ArtSharing.ViewModels;

namespace ArtSharing.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task CreatePostAsync(PostCreateViewModel model, string userId);
        Task<Post?> GetPostDetailsAsync(int id);
        Task<EditPostViewModel?> GetPostForEditAsync(int id, string userId);
        Task<bool> UpdatePostAsync(int id, EditPostViewModel model, string userId);
        Task<Post?> GetPostForDeleteAsync(int id, string userId);
        Task<bool> DeletePostConfirmedAsync(int id, string userId);
        Task<List<Comment>> LoadMoreCommentsAsync(int postId, int skip = 0);
    }
}
