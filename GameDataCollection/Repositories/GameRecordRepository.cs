﻿using GameDataCollection.DbContext;
using GameDataCollection.Models;
using Microsoft.EntityFrameworkCore;

namespace GameDataCollection.Repositories
{
    public class GameRecordRepository : BaseRepository<GameRecord>, IGameRecordRepository
    {
        private readonly UserDbContext _context;
        public GameRecordRepository(UserDbContext userDbContext, UserDbContext context) : base(userDbContext)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameRecord>> GetNonExpiredGameRecordsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.GamesRecord
                .Where(gr => gr.ExpiryDateTime.Date >= currentDate.Date.AddDays(1))
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameRecord>> GetExpiredGameRecordsAsync()
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _context.GamesRecord
                .Where(gr => gr.ExpiryDateTime.Day == currentDate.Date.Day)
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameRecord>> GetTodayGameRecordsAsync()
        {
            var currentDate = DateTime.UtcNow.Date.AddDays(-1);
            var nextDate = currentDate.AddDays(1);

            return await _context.GamesRecord
                .Where(gr => gr.CreatedDateTime >= currentDate && gr.CreatedDateTime < nextDate)
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }
    }
}
