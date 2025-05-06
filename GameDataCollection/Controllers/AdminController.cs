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

namespace GameDataCollection.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IGameRecordService _gameRecordService;
        private readonly INotyfService _notyf;

        public AdminController(SignInManager<User> signInManager, UserManager<User> userManager, IGameRecordService gameRecordService, INotyfService notyf)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _gameRecordService = gameRecordService;
            _notyf = notyf;
        }

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
                GameRecords = _gameRecordService.GetAll().Result.OrderByDescending(a=>a.Id).ToList()
            };
            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Internal Error Occurred!!");
                return View(vm);
            }
            var user = await _userManager.FindByNameAsync(vm.Username);
            if (user is null)
            {
                _notyf.Error("Invalid username or password!!");
                return View(vm);
            }
            
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, true, false);
            if (!result.Succeeded)
            {
                _notyf.Error("Invalid username or password!!");
                return View(vm);
            }
 
            var claims = new List<Claim>() {
                new(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity); 
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
            {
                IsPersistent = false
            });

            return RedirectToAction("Index", "Admin");
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
