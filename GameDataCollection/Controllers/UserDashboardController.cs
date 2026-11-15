using AspNetCoreHero.ToastNotification.Abstractions;
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
        private readonly SignInManager<User> _signInManager;
        private readonly INotyfService _notyf;

        public UserDashboardController(UserManager<User> userManager, ISpinService spinService, UserDbContext db, SignInManager<User> signInManager, INotyfService notyf)
        {
            _userManager = userManager;
            _spinService = spinService;
            _db = db;
            _signInManager = signInManager;
            _notyf = notyf;
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

            // Last few messages with the admin, for the message card
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminId = admins.FirstOrDefault()?.Id;
            var recentMessages = new List<ChatMessage>();
            if (adminId != null)
            {
                recentMessages = await _db.ChatMessages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == adminId) ||
                                (m.SenderId == adminId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.SentAt)
                    .Take(10)
                    .ToListAsync();
                recentMessages.Reverse();   // oldest first, newest at the bottom
            }

            var vm = new UserDashboardViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                GameRecord = gameRecord,
                SpinHistory = spinHistory,
                AdminId = adminId,
                UserId = userId,
                RecentMessages = recentMessages,
                SpinsRemainingToday = total,
                FreeSpinsRemaining = freeSpins,
                GrantedSpinsRemaining = grantedSpins,
                NextSpinAvailableAt = nextSpinAt
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Login");

            var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            // Changing the password updates the security stamp; refresh the cookie so the user stays logged in
            await _signInManager.RefreshSignInAsync(user);
            _notyf.Success("Password changed successfully.");
            return RedirectToAction(nameof(ChangePassword));
        }
    }
}
