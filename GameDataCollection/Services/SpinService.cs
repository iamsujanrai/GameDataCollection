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
            var setting = await _db.SpinSettings.FirstOrDefaultAsync()
                ?? new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 1 };

            var prizes = await _db.SpinPrizes
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            if (!prizes.Any())
                return new SpinResult { Success = false, Message = "No prizes configured." };

            // --- Check admin-granted spins first (independent of cooldown) ---
            // Load all grants for user into memory to avoid EF/lazy-proxy translation issues
            var allGrants = await _db.UserSpinGrants
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.GrantedAt)
                .ToListAsync();

            var grant = allGrants.FirstOrDefault(g => g.SpinsUsed < g.SpinsGranted);
            bool usedGrant = grant != null;

            if (!usedGrant)
            {
                // --- Check free-spin cooldown (only counts spins where IsGrantedSpin = false) ---
                var lastFreeSpin = await _db.SpinHistories
                    .Where(s => s.UserId == userId && !s.IsGrantedSpin)
                    .OrderByDescending(s => s.SpunAt)
                    .FirstOrDefaultAsync();

                if (lastFreeSpin != null && lastFreeSpin.SpunAt.AddHours(setting.CooldownHours) > DateTime.UtcNow)
                {
                    var nextAt = lastFreeSpin.SpunAt.AddHours(setting.CooldownHours);
                    var hoursLeft = Math.Ceiling((nextAt - DateTime.UtcNow).TotalHours);
                    return new SpinResult
                    {
                        Success = false,
                        Message = $"Next free spin available in ~{hoursLeft}h."
                    };
                }
            }
            else
            {
                grant!.SpinsUsed++;
            }

            var prize = GetRandomPrize(prizes);
            int prizeIndex = prizes.IndexOf(prize);

            _db.SpinHistories.Add(new SpinHistory
            {
                UserId = userId,
                PrizeAmount = prize.Amount,
                PrizeLabel = prize.Label,
                SegmentIndex = prizeIndex,
                SpunAt = DateTime.UtcNow,
                IsGrantedSpin = usedGrant   // track whether this consumed a grant
            });
            await _db.SaveChangesAsync();

            return new SpinResult
            {
                Success = true,
                Message = usedGrant ? "Spin completed using a bonus spin." : "Spin completed.",
                PrizeLabel = prize.Label,
                PrizeAmount = prize.Amount,
                PrizeIndex = prizeIndex,
                FinalRotationDeg = 0
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

        public async Task<(int Free, int Granted)> GetSpinsRemainingAsync(string userId)
        {
            var setting = await _db.SpinSettings.FirstOrDefaultAsync()
                ?? new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 1 };

            // Free spin: check only non-granted spins for cooldown
            var lastFreeSpin = await _db.SpinHistories
                .Where(s => s.UserId == userId && !s.IsGrantedSpin)
                .OrderByDescending(s => s.SpunAt)
                .FirstOrDefaultAsync();

            bool freeSpinAvailable = lastFreeSpin == null
                || lastFreeSpin.SpunAt.AddHours(setting.CooldownHours) <= DateTime.UtcNow;

            int free = freeSpinAvailable ? 1 : 0;

            // Granted spins: load all for this user and sum in memory
            var userGrants = await _db.UserSpinGrants
                .Where(g => g.UserId == userId)
                .ToListAsync();
            int granted = userGrants.Sum(g => Math.Max(0, g.SpinsGranted - g.SpinsUsed));

            return (free, granted);
        }

        public async Task<DateTime?> GetNextSpinAvailableAtAsync(string userId)
        {
            var setting = await _db.SpinSettings.FirstOrDefaultAsync()
                ?? new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 1 };

            // Only look at free spins (not granted) to determine next cooldown reset
            var lastFreeSpin = await _db.SpinHistories
                .Where(s => s.UserId == userId && !s.IsGrantedSpin)
                .OrderByDescending(s => s.SpunAt)
                .FirstOrDefaultAsync();

            if (lastFreeSpin == null) return null;

            var nextAt = lastFreeSpin.SpunAt.AddHours(setting.CooldownHours);
            return nextAt > DateTime.UtcNow ? nextAt : (DateTime?)null;
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
