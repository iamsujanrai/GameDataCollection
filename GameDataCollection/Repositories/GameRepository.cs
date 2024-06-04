using GameDataCollection.DbContext;
using GameDataCollection.Models;

namespace GameDataCollection.Repositories
{
    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        public GameRepository(UserDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
