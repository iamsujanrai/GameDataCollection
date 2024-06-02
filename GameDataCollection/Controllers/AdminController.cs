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

namespace GameDataCollection.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IGameRecordService _gameRecordService;

        public AdminController(SignInManager<User> signInManager, UserManager<User> userManager, IGameRecordService gameRecordService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _gameRecordService = gameRecordService;
        }

        public IActionResult Index()
        {
            var vm = new ReportViewModel
            {
                GameRecords = _gameRecordService.GetNonExpiredGameRecordsAsync().Result.ToList()
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
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            var user = await _userManager.FindByNameAsync(vm.Username) ?? throw new Exception("Invalid username or password");
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, true, false);

            if (!result.Succeeded)
            {
                throw new Exception("Invalid username or password");
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
                throw new Exception("User id is null");
            }

            var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
            var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                throw new Exception("Password mismatch");
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return View();
        }
    }
}
