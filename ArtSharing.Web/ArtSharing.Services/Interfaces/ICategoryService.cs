using ArtSharing.Data.Models.Models;

namespace ArtSharing.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
