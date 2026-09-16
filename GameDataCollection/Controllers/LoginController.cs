using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.Extension;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet]
        [Route("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Please enter a valid email address.");
                return View(vm);
            }

            var normalized = _userManager.NormalizeEmail(vm.Email.Trim());
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized || u.NormalizedUserName == normalized);

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action("ResetPassword", "Login", new { userId = user.Id, token }, Request.Scheme);
                var to = user.Email;
                var body = EmailSender.PasswordResetTemplate(user.FullName, resetUrl);

                // Send in the background so the response time doesn't reveal whether the account exists
                _ = Task.Run(() => EmailSender.EmailSend(to, "Reset your Aliyah Gaming password", body));
            }

            // Same message whether or not the account exists, so this page can't be used to find registered emails
            ViewBag.Sent = true;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _notyf.Error("This password reset link is invalid.");
                return RedirectToAction(nameof(ForgotPassword));
            }
            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByIdAsync(vm.UserId);
            var result = user == null
                ? IdentityResult.Failed(new IdentityError { Code = "InvalidToken" })
                : await _userManager.ResetPasswordAsync(user, vm.Token, vm.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    _notyf.Error("This password reset link is invalid or has expired. Please request a new one.");
                    return RedirectToAction(nameof(ForgotPassword));
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            // Unlock the account in case it was locked by earlier failed logins
            await _userManager.SetLockoutEndDateAsync(user!, null);
            await _userManager.ResetAccessFailedCountAsync(user!);

            _notyf.Success("Your password has been reset. Please log in.");
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [Route("AccessDenied")]
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
