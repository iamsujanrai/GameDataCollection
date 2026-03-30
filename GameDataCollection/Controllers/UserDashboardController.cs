using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameDataCollection.Controllers
{
    [Authorize(Roles = "User")]
    public class UserDashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ISpinService _spinService;
        private readonly UserDbContext _db;

        public UserDashboardController(UserManager<User> userManager, ISpinService spinService, UserDbContext db)
        {
            _userManager = userManager;
            _spinService = spinService;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Match GameRecord by email
            var gameRecord = await _db.GamesRecord
                .Include(g => g.Game)
                .Include(g => g.State)
                .FirstOrDefaultAsync(g => g.Email.ToLower() == user.Email.ToLower());

            var spinHistory = await _spinService.GetSpinHistoryAsync(userId);
            var (freeSpins, grantedSpins) = await _spinService.GetSpinsRemainingAsync(userId);
            int total = freeSpins + grantedSpins;
            DateTime? nextSpinAt = total == 0
                ? await _spinService.GetNextSpinAvailableAtAsync(userId)
                : null;

            var vm = new UserDashboardViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                GameRecord = gameRecord,
                SpinHistory = spinHistory,
                SpinsRemainingToday = total,
                FreeSpinsRemaining = freeSpins,
                GrantedSpinsRemaining = grantedSpins,
                NextSpinAvailableAt = nextSpinAt
            };

            return View(vm);
        }
    }
}
