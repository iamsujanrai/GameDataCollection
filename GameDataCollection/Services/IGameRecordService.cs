using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public interface IGameRecordService
    {
        Task Save(GameRecord gameRecord);
        Task<List<GameRecord>> GetAll();
        Task<IEnumerable<GameRecord>> GetNonExpiredGameRecordsAsync();
        Task<IEnumerable<GameRecord>> GetExpiredGameRecordsAsync();
    }
}
