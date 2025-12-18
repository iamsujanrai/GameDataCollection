using System.Security.Cryptography;
using System;
using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public class SpinService : ISpinService
    {
        // You can move this to DB or config later
        private readonly List<PrizeOption> _prizes = new()
        {
            new PrizeOption { Index = 0, Label = "$1",  Amount = 1,  Weight = 40 },
            new PrizeOption { Index = 1, Label = "$2",  Amount = 2,  Weight = 30 },
            new PrizeOption { Index = 2, Label = "$5",  Amount = 5,  Weight = 15 },
            new PrizeOption { Index = 3, Label = "$10", Amount = 10, Weight = 10 },
            new PrizeOption { Index = 4, Label = "$20", Amount = 20, Weight = 4 },
            new PrizeOption { Index = 5, Label = "$50", Amount = 50, Weight = 1 },
        };

        //private readonly ApplicationDbContext _db;
        private readonly ILogger<SpinService> _logger;

        public SpinService(/*ApplicationDbContext db, */ILogger<SpinService> logger)
        {
            //_db = db;
            _logger = logger;
        }

        public async Task<SpinResult> SpinAsync(string userId)
        {
            // 🔒 Example: limit spins per day
            var today = DateTime.UtcNow.Date;
            int maxDailySpins = 5;

            //int usedSpins = await _db.Spins
            //    .Where(s => s.UserId == userId && s.CreatedAt >= today)
            //    .CountAsync();

            //if (usedSpins >= maxDailySpins)
            //{
            //    return new SpinResult
            //    {
            //        Success = false,
            //        Message = "Daily spin limit reached."
            //    };
            //}

            // 🎲 Choose prize using weighted randomness
            var prize = GetRandomPrize();

            // 🧮 Calculate wheel rotation so pointer ends on this prize
            int totalSlices = _prizes.Count;
            double sliceAngle = 360.0 / totalSlices;
            double fullSpins = RandomNumberGenerator.GetInt32(5, 10); // 5–9 full turns
            double offsetToCenter = sliceAngle / 2.0;
            double finalRotation = fullSpins * 360.0 + prize.Index * sliceAngle + offsetToCenter;

            //// 💾 Save spin to DB (simplified)
            //_db.Spins.Add(new Spin
            //{
            //    UserId = userId,
            //    PrizeAmount = prize.Amount,
            //    PrizeLabel = prize.Label,
            //    SegmentIndex = prize.Index,
            //    CreatedAt = DateTime.UtcNow
            //});
            //await _db.SaveChangesAsync();

            return new SpinResult
            {
                Success = true,
                Message = "Spin completed.",
                PrizeLabel = prize.Label,
                PrizeAmount = prize.Amount,
                PrizeIndex = prize.Index,
                FinalRotationDeg = finalRotation
            };
        }

        private PrizeOption GetRandomPrize()
        {
            int totalWeight = _prizes.Sum(p => p.Weight);
            int randomNumber = RandomNumberGenerator.GetInt32(0, totalWeight);

            int cumulative = 0;
            foreach (var p in _prizes)
            {
                cumulative += p.Weight;
                if (randomNumber < cumulative)
                    return p;
            }

            return _prizes.Last();
        }
    }
}
