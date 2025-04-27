using ArtSharing.Data.Models.Models;
using ArtSharing.ViewModels;
    
namespace ArtSharing.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task CreateCategoryAsync(string name, string description);
        Task<Category?> GetCategoryByIdAsync(int id);
        Task EditCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);

        Task<List<UserWithRolesViewModel>> GetAllUsersWithRolesAsync();
        Task PromoteToModeratorAsync(string id);
        Task RemoveModeratorRoleAsync(string id);

        Task<List<Report>> GetAllReportsAsync();
        Task BanUserAsync(string userId);
        Task BanUserFromListAsync(string id);
        Task UnbanUserAsync(string id);

        Task<bool> DeletePostAsync(int postId);
        Task DeleteCommentAsync(int commentId);
        Task DismissReportAsync(int reportId);
    }
}
