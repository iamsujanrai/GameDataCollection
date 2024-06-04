using GameDataCollection.Models;
using GameDataCollection.Repositories;

namespace GameDataCollection.Services
{
    public interface IGameService
    {
        Task Save(Game game);
        Game GetById(long id);
        Task<List<Game>> GetAll();
        void Delete(Game game);
    }
}
