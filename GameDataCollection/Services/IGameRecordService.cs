using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public interface IGameRecordService
    {
        Task Save(GameRecord gameRecord);
    }
}
