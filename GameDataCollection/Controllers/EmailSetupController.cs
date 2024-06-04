using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameDataCollection.Controllers
{
    public class EmailSetupController : Controller
    {
        private readonly IEmailSetupService _emailService;
        private readonly INotyfService _notyf;

        public EmailSetupController(IEmailSetupService emailService, INotyfService notyf)
        {
            _emailService = emailService;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new EmailIndexViewModel
            {
                Emails = await _emailService.GetAll()
            };
            return View(vm);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmailSetupViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Internal error occurred!!");
                    return View(vm);
                }

                var email = CreateModelFromViewModel(vm);
                _emailService.Save(email);
                _notyf.Success("Email successfully added.");
                return RedirectToAction(nameof(Index), "EmailSetup");
            }
            catch (Exception)
            {
                _notyf.Error("Internal error occurred!!");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult ToggleStatus(long id)
        {
            try
            {
                var email = _emailService.GetById(id);
                if (email is null)
                {
                    _notyf.Error("Email not found!!");
                    return RedirectToAction(nameof(Index), "EmailSetup");
                }

                _emailService.ToggleStatus(email);
                _notyf.Success("Email status changed successfully");
                return RedirectToAction(nameof(Index), "EmailSetup");
            }
            catch (Exception)
            {
                _notyf.Error("Internal error occurred!!");
                return RedirectToAction(nameof(Index), "EmailSetup");
            }
        }

        private static Email CreateModelFromViewModel(EmailSetupViewModel vm)
        {
            return new Email
            {
                MemberEmail = vm.MemberEmail
            };
        }
    }
}
