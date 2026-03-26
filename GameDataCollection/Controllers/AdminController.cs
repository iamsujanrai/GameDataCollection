using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Security.Claims;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GameDataCollection.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IGameRecordService _gameRecordService;
        private readonly INotyfService _notyf;
        private readonly UserDbContext _db;

        public AdminController(SignInManager<User> signInManager, UserManager<User> userManager, IGameRecordService gameRecordService, INotyfService notyf, UserDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _gameRecordService = gameRecordService;
            _notyf = notyf;
            _db = db;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var vm = new ReportViewModel
            {
                GameRecords = _gameRecordService.GetTodayRecordsAsync().Result.ToList()
            };
            return View(vm);
        }

        public IActionResult ExpiredReport()
        {
            var vm = new ReportViewModel
            {
                GameRecords = _gameRecordService.GetExpiredGameRecordsAsync().Result.ToList()
            };
            return View(vm);
        }
        public IActionResult AllReport()
        {
            var vm = new ReportViewModel
            {
                GameRecords = _gameRecordService.GetAll().Result.OrderByDescending(a => a.Id).ToList()
            };
            return View(vm);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.User);
                    _notyf.Success("User registered successfully.");
                    return RedirectToAction("UserList", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        public async Task<IActionResult> UserList(int page = 1, string? search = null)
        {
            int pageSize = 10;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var totalUsers = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize),
                Search = search
            };

            return View(model);
        }

        // ── Spin Settings ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SpinSettings()
        {
            var setting = await _db.SpinSettings.FirstOrDefaultAsync()
                ?? new SpinSetting { CooldownHours = 24, MaxSpinsPerPeriod = 5 };

            var vm = new SpinSettingViewModel
            {
                Id = setting.Id,
                CooldownHours = setting.CooldownHours,
                MaxSpinsPerPeriod = setting.MaxSpinsPerPeriod
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpinSettings(SpinSettingViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var setting = await _db.SpinSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new SpinSetting();
                _db.SpinSettings.Add(setting);
            }
            setting.CooldownHours = vm.CooldownHours;
            setting.MaxSpinsPerPeriod = vm.MaxSpinsPerPeriod;
            await _db.SaveChangesAsync();

            _notyf.Success("Spin settings saved.");
            return RedirectToAction(nameof(SpinSettings));
        }

        // ── Spin Prizes ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SpinPrizes()
        {
            var prizes = await _db.SpinPrizes.OrderBy(p => p.SortOrder).ToListAsync();
            return View(prizes);
        }

        [HttpGet]
        public IActionResult CreateSpinPrize() => View(new SpinPrizeViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpinPrize(SpinPrizeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            _db.SpinPrizes.Add(new SpinPrize
            {
                Label = vm.Label,
                Amount = vm.Amount,
                Weight = vm.Weight,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive
            });
            await _db.SaveChangesAsync();
            _notyf.Success("Prize added.");
            return RedirectToAction(nameof(SpinPrizes));
        }

        [HttpGet]
        public async Task<IActionResult> EditSpinPrize(int id)
        {
            var prize = await _db.SpinPrizes.FindAsync(id);
            if (prize == null) return NotFound();

            var vm = new SpinPrizeViewModel
            {
                Id = prize.Id,
                Label = prize.Label,
                Amount = prize.Amount,
                Weight = prize.Weight,
                SortOrder = prize.SortOrder,
                IsActive = prize.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpinPrize(SpinPrizeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var prize = await _db.SpinPrizes.FindAsync(vm.Id);
            if (prize == null) return NotFound();

            prize.Label = vm.Label;
            prize.Amount = vm.Amount;
            prize.Weight = vm.Weight;
            prize.SortOrder = vm.SortOrder;
            prize.IsActive = vm.IsActive;
            await _db.SaveChangesAsync();

            _notyf.Success("Prize updated.");
            return RedirectToAction(nameof(SpinPrizes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpinPrize(int id)
        {
            var prize = await _db.SpinPrizes.FindAsync(id);
            if (prize != null)
            {
                _db.SpinPrizes.Remove(prize);
                await _db.SaveChangesAsync();
                _notyf.Success("Prize deleted.");
            }
            return RedirectToAction(nameof(SpinPrizes));
        }

        // ── Grant Spins ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GrantSpins()
        {
            var vm = new GrantSpinsViewModel
            {
                Users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantSpins(GrantSpinsViewModel vm)
        {
            vm.Users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            if (!ModelState.IsValid) return View(vm);

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.UserSpinGrants.Add(new UserSpinGrant
            {
                UserId = vm.UserId,
                SpinsGranted = vm.SpinsToGrant,
                SpinsUsed = 0,
                GrantedAt = DateTime.UtcNow,
                Note = vm.Note,
                GrantedByAdminId = adminId
            });
            await _db.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(vm.UserId);
            _notyf.Success($"Granted {vm.SpinsToGrant} spin(s) to {user?.FullName ?? user?.Email}.");
            return RedirectToAction(nameof(GrantSpins));
        }
        //[AllowAnonymous]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel vm)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        _notyf.Error("Internal Error Occurred!!");
        //        return View(vm);
        //    }

        //    var user = await _userManager.FindByNameAsync(vm.Username);
        //    if (user == null)
        //    {
        //        _notyf.Error("Invalid username or password!!");
        //        return View(vm);
        //    }

        //    var result = await _signInManager.PasswordSignInAsync(
        //        vm.Username,
        //        vm.Password,
        //        isPersistent: true,
        //        lockoutOnFailure: true
        //    );
        //    if (result.IsLockedOut)
        //    {
        //        _notyf.Error("Account locked out.");
        //        return View(vm);
        //    }

        //    if (result.IsNotAllowed)
        //    {
        //        _notyf.Error("Login not allowed (email not confirmed or disabled).");
        //        return View(vm);
        //    }

        //    if (result.RequiresTwoFactor)
        //    {
        //        _notyf.Warning("Two-factor authentication required.");
        //        return View(vm);
        //    }

        //    if (!result.Succeeded)
        //    {
        //        _notyf.Error("Invalid username or password.");
        //        return View(vm);
        //    }

        //    // 🔐 ROLE-BASED REDIRECT
        //    if (await _userManager.IsInRoleAsync(user, "Admin"))
        //        return RedirectToAction("Index", "Admin");

        //    return RedirectToAction("Index", "UserDashboard");
        //}
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) as string;
            if (userId == null)
            {
                _notyf.Error("Cannot find logged in user id!!");
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
            var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                _notyf.Error("Password mismatch!!");
                return View(vm);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            _notyf.Success("Password changed successfully.");
            return View();
        }
    }
}
