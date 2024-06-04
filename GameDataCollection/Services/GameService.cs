using GameDataCollection.Models;
using GameDataCollection.Repositories;

namespace GameDataCollection.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public Task<List<Game>> GetAll()
        {
            try
            {
                return _gameRepository.GetAll();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Save(Game game)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(game);

                await _gameRepository.InsertAsync(game);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Game GetById(long id)
        {
            try
            {
                return _gameRepository.GetById(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Delete(Game game)
        {
            try
            {
                _gameRepository.Delete(game);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
