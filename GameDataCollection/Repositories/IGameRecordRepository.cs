using GameDataCollection.Models;

namespace GameDataCollection.Repositories
{
    public interface IGameRecordRepository : IBaseRepository<GameRecord>
    {
        Task<IEnumerable<GameRecord>> GetNonExpiredGameRecordsAsync();
        Task<IEnumerable<GameRecord>> GetExpiredGameRecordsAsync();
        Task<IEnumerable<GameRecord>> GetTodayGameRecordsAsync();
    }
}
