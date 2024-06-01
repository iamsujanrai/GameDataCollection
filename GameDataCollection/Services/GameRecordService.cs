﻿using GameDataCollection.Models;
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
    }
}
