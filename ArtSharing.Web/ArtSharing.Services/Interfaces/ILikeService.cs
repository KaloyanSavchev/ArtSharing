using ArtSharing.ViewModels;

namespace ArtSharing.Services.Interfaces
{
    public interface ILikeService
    {
        Task<(bool hasLiked, int likeCount)> ToggleLikeAsync(LikeDto data, string userId);
    }
}
