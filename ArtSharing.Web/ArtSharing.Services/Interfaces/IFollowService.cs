using System.Threading.Tasks;

namespace ArtSharing.Services.Interfaces
{
    public interface IFollowService
    {
        Task<bool> ToggleFollowAsync(string followerId, string followingId);
    }
}
