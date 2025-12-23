using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace GameDataCollection.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpinController : ControllerBase
    {
        // ✅ THIS is where items come from
        private static readonly List<SpinItem> Items = new()
    {
        new SpinItem { Id = 1, Title = "$5 Bonus",  Weight = 50, Amount = 5 },
        new SpinItem { Id = 2, Title = "$10 Bonus", Weight = 0, Amount = 10 },
        new SpinItem { Id = 3, Title = "Try Again", Weight = 50, Amount = 0 },
        new SpinItem { Id = 4, Title = "11 Bonus", Weight = 0, Amount = 11 },
        new SpinItem { Id = 5, Title = "12 Bonus", Weight = 0, Amount = 12 },
        new SpinItem { Id = 6, Title = "13 Bonus", Weight = 0, Amount = 13},
        new SpinItem { Id = 7, Title = "14 Bonus", Weight = 0, Amount = 14 },
        new SpinItem { Id = 8, Title = "15 Bonus", Weight = 0, Amount = 15 },
    };

        [HttpGet("list")]
        public IActionResult GetSpinList()
        {
            return new JsonResult(Items);
        }
        // POST: /api/spin/generate
        [HttpPost("generate")]
        public IActionResult Generate()
        {
            var winner = PickWeighted(Items);

            return new JsonResult(new
            {
                rewardId = winner.Id,
                title = winner.Title,
                amount = winner.Amount
            });
        }

        private static SpinItem PickWeighted(List<SpinItem> items)
        {
            var total = items.Sum(x => x.Weight);
            if (total <= 0) return items[0];

            var roll = RandomNumberGenerator.GetInt32(1, total + 1);

            int current = 0;
            foreach (var item in items)
            {
                current += item.Weight;
                if (roll <= current) return item;
            }

            return items[0];
        }

        private class SpinItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public int Weight { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
