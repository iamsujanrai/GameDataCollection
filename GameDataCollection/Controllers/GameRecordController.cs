using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameDataCollection.Controllers
{
    public class GameRecordController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IGameRecordService _gameRecordService;
        private readonly INotyfService _notyf;

        public GameRecordController(UserDbContext context, IGameRecordService gameRecordService, INotyfService notyf)
        {
            _context = context;
            _gameRecordService = gameRecordService;
            _notyf = notyf;
        }

        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameRecordViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Internal Error Occurred!!");
                }
                var gameRecord = GetGameRecordFromVM(vm);
                await _gameRecordService.Save(gameRecord);

                _notyf.Success("Record successfully added.");
                return View(vm);
            }
            catch (Exception)
            {
                throw;
            }
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
    }
}
