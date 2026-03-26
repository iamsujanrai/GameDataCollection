using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public interface ISpinService
    {
        Task<SpinResult> SpinAsync(string userId);
        Task<List<SpinHistory>> GetSpinHistoryAsync(string userId);
        Task<List<SpinPrize>> GetActiveSpinPrizesAsync();
    }
}
