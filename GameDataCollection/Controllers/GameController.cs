using AspNetCoreHero.ToastNotification.Abstractions;
using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.Services;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace GameDataCollection.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;
        private readonly INotyfService _notfy;

        public GameController(IGameService gameService, INotyfService notfy)
        {
            _gameService = gameService;
            _notfy = notfy;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new GameIndexViewModel
            {
                Games = await _gameService.GetAll()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notfy.Error("Internal Error Occurred!!");
                    return View(vm);
                }
                var game = GetGameFromVM(vm);
                await _gameService.Save(game);

                _notfy.Success("Game added successfully.");
                return RedirectToAction(nameof(Index), "Game");
            }
            catch (Exception)
            {
                _notfy.Error("Internal Error Occurred!!");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            try
            {
                var game = _gameService.GetById(id);
                if (game == null)
                {
                    _notfy.Error("Game not found!!");
                    return Redirect("Index");
                }

                _gameService.Delete(game);
                _notfy.Success("Game successfully deleted.");

                return RedirectToAction(nameof(Index), "Game");
            }
            catch (Exception)
            {
                _notfy.Error("Internal Error Occurred!!");
                return RedirectToAction(nameof(Index), "Game");
            }
        }

        private static Game GetGameFromVM(GameViewModel vm)
        {
            return new Game { Name = vm.Name };
        }
    }
}
