using GameDataCollection.DbContext;
using GameDataCollection.Models;

namespace GameDataCollection.Repositories
{
    public class EmailSetupRepository : BaseRepository<Email>, IEmailSetupRepository
    {
        public EmailSetupRepository(UserDbContext userDbContext): base(userDbContext)
        {
            
        }
    }
}
