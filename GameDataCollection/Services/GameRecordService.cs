using GameDataCollection.Models;
using GameDataCollection.Repositories;
using GameDataCollection.ViewModels;

namespace GameDataCollection.Services
{
    public class GameRecordService : IGameRecordService
    {
		private readonly IGameRecordRepository _gameRecordRepository;

        public GameRecordService(IGameRecordRepository gameRecordRepository)
        {
            _gameRecordRepository = gameRecordRepository;
        }

        public Task<List<GameRecord>> GetAll()
        {
            try
            {
                return _gameRecordRepository.GetAll();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<GameRecord>> GetNonExpiredGameRecordsAsync()
        {
            return await _gameRecordRepository.GetNonExpiredGameRecordsAsync();
        }

        public async Task<IEnumerable<GameRecord>> GetExpiredGameRecordsAsync()
        {
            return await _gameRecordRepository.GetExpiredGameRecordsAsync();
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

        public void Edit(GameRecord record)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(record);

                _gameRecordRepository.Update(record);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(GameRecord record)
        {
            try
            {
               _gameRecordRepository.Delete(record);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GameRecord getById(int id)
        {
            try
            {
                var a = _gameRecordRepository.GetById(id);
                return _gameRecordRepository.GetQueryable().Where(a=>a.Id.Equals(id)).SingleOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GameRecord IsRecordExists(GameRecordViewModel record)
        {
            try
            {
                return _gameRecordRepository.GetQueryable().Where(a => a.Email.ToLower().Equals(record.Email.ToLower()) || a.PhoneNumber.Equals(record.PhoneNumber)).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
