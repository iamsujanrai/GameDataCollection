using GameDataCollection.Models;
using GameDataCollection.ViewModels;

namespace GameDataCollection.Services
{
    public interface IGameRecordService
    {
        GameRecord getById(int id);
        void Edit(GameRecord record);
        void Delete(GameRecord record);
        Task Save(GameRecord gameRecord);
        Task<List<GameRecord>> GetAll();
        Task<IEnumerable<GameRecord>> GetNonExpiredGameRecordsAsync();
        Task<IEnumerable<GameRecord>> GetExpiredGameRecordsAsync();
        Task<IEnumerable<GameRecord>> GetTodayRecordsAsync();
        GameRecord IsRecordExists(GameRecordViewModel record);
    }
}
