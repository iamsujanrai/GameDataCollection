using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public interface ISpinService
    {
        Task<SpinResult> SpinAsync(string userId);
    }
}
