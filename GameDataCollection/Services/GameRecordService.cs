using GameDataCollection.Models;
using GameDataCollection.Repositories;

namespace GameDataCollection.Services
{
    public class GameRecordService : IGameRecordService
    {
		private readonly IGameRecordRepository _gameRecordRepository;

        public GameRecordService(IGameRecordRepository gameRecordRepository)
        {
            _gameRecordRepository = gameRecordRepository;
        }

        public async Task Save(GameRecord gameRecord)
        {
			try
			{
                ArgumentNullException.ThrowIfNull(gameRecord);

                await _gameRecordRepository.InsertAsync(gameRecord);
			}
			catch (Exception)
			{
				throw;
			}
        }
    }
}
