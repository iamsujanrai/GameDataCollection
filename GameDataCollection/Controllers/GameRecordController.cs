using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.DbContext;
using GameDataCollection.Extension;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace GameDataCollection.Controllers
{
    public class GameRecordController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IGameRecordService _gameRecordService;
        private readonly INotyfService _notyf;
        private readonly IEmailSetupService _emailSetupService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly UserManager<User> _userManager;

        public GameRecordController(UserDbContext context, IGameRecordService gameRecordService, INotyfService notyf,IEmailSetupService emailSetupService, IHttpClientFactory clientFactory, UserManager<User> userManager)
        {
            _context = context;
            _gameRecordService = gameRecordService;
            _notyf = notyf;
            _emailSetupService = emailSetupService;
            _clientFactory = clientFactory;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(Create());
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult Create()
        {
            var vm = new GameRecordViewModel
            {
                Games = GetGames(),
                States = GetStates()
            };
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameRecordViewModel vm)
        {
            try
            {
                // Check ModelState first
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Invalid form data submitted.");
                    vm.Games = GetGames();
                    vm.States = GetStates();
                    return View(vm);
                }

                // CAPTCHA Verification
                var recaptchaResponse = Request.Form["g-recaptcha-response"];
                var secretKey = "6LeK_i0rAAAAAK69ipD4G4a2mE1dMsdnrMEzd3ne"; // Replace this with your actual key

                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}", null);
                var jsonString = await response.Content.ReadAsStringAsync();

                var captchaResult = JsonSerializer.Deserialize<GoogleCaptchaResponse>(jsonString);

                if (captchaResult == null || !captchaResult.success)
                {
                    _notyf.Error("Captcha validation failed. Please try again.");
                    vm.Games = GetGames();
                    vm.States = GetStates();
                    return View(vm);
                }

                // Check for duplicate record
                var recordExists = _gameRecordService.IsRecordExists(vm);
                if (recordExists != null)
                {
                    _notyf.Error("Record already exists!");
                    return RedirectToAction("Create");
                }

                // Save data
                var gameRecord = GetGameRecordFromVM(vm);
                await _gameRecordService.Save(gameRecord);

                // Create a login account for the user
                var password = await CreateUserAccount(vm);

                // Send emails
                _notyf.Success("Thanks for registering!");
                if (password != null)
                {
                    var loginUrl = Url.Action("Login", "Login", null, Request.Scheme);
                    EmailSender.EmailSend(vm.Email.Trim(), "Your Aliyah Gaming account", EmailSender.AccountCreatedTemplate(vm.FullName, vm.Email.Trim(), password, loginUrl));
                }
                else
                {
                    EmailSender.EmailSend(vm.Email, "Thank you for registering", EmailSender.RegisterTemplate(vm.FullName));
                }

                var listOfEmail = _emailSetupService.GetAll().Result.Where(a => a.IsActive).ToList();
                // You can uncomment this if you want to notify internal members
                //foreach (var item in listOfEmail)
                //{
                //    EmailSender.EmailSend(item.MemberEmail, $"{vm.FullName} registered successfully", EmailSender.RegisterTemplate(vm.FullName));
                //}

                return RedirectToAction("Congratulation");
            }
            catch (Exception ex)
            {
                // Log exception here if possible
                _notyf.Error("An unexpected error occurred.");
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var record = _gameRecordService.getById(id);
            var vm=Copy(record);
            return View(vm);
        }
        private GameRecordViewModel Copy(GameRecord record)
        {
            var gameRecordVm = new GameRecordViewModel()
            {
                Id=record.Id,
                Email = record.Email,
                FacebookName=record.FacebookName,
                FullName=record.FullName,
                GameId=record.GameId,
                GameUserId=record.GameUserId,
                PhoneNumber=record.PhoneNumber,
                RefferedBy=record.RefferedBy,
                StateId=record.StateId,
                CreatedDateTime=record.CreatedDateTime,
                ExpiryDateTime=record.ExpiryDateTime,
                Games=GetGames(),
                States=GetStates()
            };
            return gameRecordVm;
        }
        private void CopyFromGameRecordViewModel(GameRecordViewModel vm, GameRecord record)
        {
            record.Id = (int)vm.Id;
            record.FullName = vm.FullName;
            record.PhoneNumber = vm.PhoneNumber;
            record.RefferedBy = vm.RefferedBy;
            record.Email = vm.Email;
            record.FacebookName = vm.FacebookName;
            record.GameUserId = vm.GameUserId;
            record.GameId = vm.GameId;
            record.StateId = vm.StateId;
            record.CreatedDateTime=Convert.ToDateTime(vm.CreatedDateTime);
            record.ExpiryDateTime=Convert.ToDateTime(vm.ExpiryDateTime);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameRecordViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Internal Error Occurred!!");
                }
                var record = _gameRecordService.getById((int)vm.Id);
                CopyFromGameRecordViewModel(vm,record);
                _gameRecordService.Edit(record);
                _notyf.Success("Edit Sucessfully");
                return Redirect("/admin/index");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var vm = new GameRecordViewModel
            {
                Games = GetGames(),
                States = GetStates()
            };
            var record = _gameRecordService.getById(id);
            _gameRecordService.Delete(record);
            _notyf.Success("Deleted sucessfully");
            return Redirect("/admin/index");
        }

        // Returns the generated password, or null if an account already exists or couldn't be created
        private async Task<string?> CreateUserAccount(GameRecordViewModel vm)
        {
            var email = vm.Email.Trim();
            var normalized = _userManager.NormalizeEmail(email);
            var exists = await _userManager.Users
                .AnyAsync(u => u.NormalizedEmail == normalized || u.NormalizedUserName == normalized);
            if (exists) return null;

            var user = new User
            {
                UserName = email,
                Email = email,
                FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber,
                EmailConfirmed = true
            };

            var password = GeneratePassword();
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return null;

            await _userManager.AddToRoleAsync(user, Roles.User);
            return password;
        }

        // 10 characters with at least one uppercase, lowercase, digit and symbol (Identity's default rules).
        // Look-alike characters such as 0/O and 1/l/I are left out so the password is easy to type.
        private static string GeneratePassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "@#$%*!";

            var chars = RandomNumberGenerator.GetItems<char>(upper + lower + digits + symbols, 6)
                .Concat(RandomNumberGenerator.GetItems<char>(upper, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(lower, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(digits, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(symbols, 1))
                .ToArray();
            RandomNumberGenerator.Shuffle<char>(chars);
            return new string(chars);
        }

        private static GameRecord GetGameRecordFromVM(GameRecordViewModel vm)
        {
            return new GameRecord()
            {
                FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber,
                RefferedBy = vm.RefferedBy,
                Email = vm.Email,
                FacebookName = vm.FacebookName,
                GameUserId = vm.GameUserId,
                GameId = vm.GameId,
                StateId = vm.StateId
            };
        }

        private List<SelectListItem> GetStates()
        {
            return [.. _context.States.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })];
        }

        private List<SelectListItem> GetGames()
        {
            return [.. _context.Games.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            })];
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Congratulation()
        {
            return View();
        }
    }
    public class GoogleCaptchaResponse
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> errorCodes { get; set; }
    }
}
