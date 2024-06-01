using GameDataCollection.DbContext;
using GameDataCollection.Models;

namespace GameDataCollection.Repositories
{
    public class GameRecordRepository : BaseRepository<GameRecord>, IGameRecordRepository
    {
        public GameRecordRepository(UserDbContext userDbContext) : base(userDbContext) { }
    }
}
