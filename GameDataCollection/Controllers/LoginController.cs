using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace GameDataCollection.Controllers
{
    [Route("Account")]
    public class LoginController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IGameRecordService _gameRecordService;
        private readonly INotyfService _notyf;

        public LoginController(SignInManager<User> signInManager, UserManager<User> userManager, IGameRecordService gameRecordService, INotyfService notyf)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _gameRecordService = gameRecordService;
            _notyf = notyf;
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
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Internal Error Occurred!!");
                return View(vm);
            }

            var user = await _userManager.FindByNameAsync(vm.Username);
            if (user == null)
            {
                _notyf.Error("Invalid username or password!!");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                vm.Username,
                vm.Password,
                isPersistent: true,
                lockoutOnFailure: true
            );
            if (result.IsLockedOut)
            {
                _notyf.Error("Account locked out.");
                return View(vm);
            }

            if (result.IsNotAllowed)
            {
                _notyf.Error("Login not allowed (email not confirmed or disabled).");
                return View(vm);
            }

            if (result.RequiresTwoFactor)
            {
                _notyf.Warning("Two-factor authentication required.");
                return View(vm);
            }

            if (!result.Succeeded)
            {
                _notyf.Error("Invalid username or password.");
                return View(vm);
            }

            // 🔐 ROLE-BASED REDIRECT
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "UserDashboard");
        }
        [HttpPost]
        [Route("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
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
