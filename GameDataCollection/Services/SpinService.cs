using System.Security.Cryptography;
using GameDataCollection.DbContext;
using GameDataCollection.Models;
using Microsoft.EntityFrameworkCore;

namespace GameDataCollection.Services
{
    public class SpinService : ISpinService
    {
        private readonly UserDbContext _db;
        private readonly ILogger<SpinService> _logger;

        public SpinService(UserDbContext db, ILogger<SpinService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<SpinResult> SpinAsync(string userId)
        {
            // Load settings (fallback if not seeded yet)
            var setting = await _db.SpinSettings.FirstOrDefaultAsync()
                ?? new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 5 };

            // Load active prizes ordered by SortOrder
            var prizes = await _db.SpinPrizes
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            if (!prizes.Any())
                return new SpinResult { Success = false, Message = "No prizes configured." };

            // Check cooldown window
            var windowStart = DateTime.UtcNow.AddHours(-setting.CooldownHours);
            int usedSpins = await _db.SpinHistories
                .Where(s => s.UserId == userId && s.SpunAt >= windowStart)
                .CountAsync();

            bool usedGrant = false;
            if (usedSpins >= setting.MaxSpinsPerPeriod)
            {
                // Check for admin-granted bonus spins
                var grant = await _db.UserSpinGrants
                    .Where(g => g.UserId == userId && g.SpinsUsed < g.SpinsGranted)
                    .OrderBy(g => g.GrantedAt)
                    .FirstOrDefaultAsync();

                if (grant == null)
                {
                    int hoursLeft = (int)Math.Ceiling(
                        (windowStart.AddHours(setting.CooldownHours) - DateTime.UtcNow).TotalHours);
                    return new SpinResult
                    {
                        Success = false,
                        Message = $"Spin limit reached. Next spin available in ~{hoursLeft}h."
                    };
                }

                grant.SpinsUsed++;
                usedGrant = true;
            }

            // Choose prize using weighted randomness
            var prize = GetRandomPrize(prizes);
            int prizeIndex = prizes.IndexOf(prize);

            // Save spin to DB
            _db.SpinHistories.Add(new SpinHistory
            {
                UserId = userId,
                PrizeAmount = prize.Amount,
                PrizeLabel = prize.Label,
                SegmentIndex = prizeIndex,
                SpunAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return new SpinResult
            {
                Success = true,
                Message = usedGrant ? "Spin completed using a bonus spin." : "Spin completed.",
                PrizeLabel = prize.Label,
                PrizeAmount = prize.Amount,
                PrizeIndex = prizeIndex,
                FinalRotationDeg = 0 // client calculates rotation from PrizeIndex
            };
        }

        public async Task<List<SpinHistory>> GetSpinHistoryAsync(string userId)
        {
            return await _db.SpinHistories
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SpunAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<SpinPrize>> GetActiveSpinPrizesAsync()
        {
            return await _db.SpinPrizes
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();
        }

        private static SpinPrize GetRandomPrize(List<SpinPrize> prizes)
        {
            int totalWeight = prizes.Sum(p => p.Weight);
            int randomNumber = RandomNumberGenerator.GetInt32(0, totalWeight);

            int cumulative = 0;
            foreach (var p in prizes)
            {
                cumulative += p.Weight;
                if (randomNumber < cumulative)
                    return p;
            }
            return prizes.Last();
        }
    }
}
