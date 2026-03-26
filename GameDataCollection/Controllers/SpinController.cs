using GameDataCollection.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameDataCollection.Controllers
{
    [Authorize(Roles = "User")]
    [AutoValidateAntiforgeryToken]
    public class SpinController : Controller
    {
        private readonly ISpinService _spinService;

        public SpinController(ISpinService spinService)
        {
            _spinService = spinService;
        }

        public async Task<IActionResult> Index()
        {
            var prizes = await _spinService.GetActiveSpinPrizesAsync();
            return View(prizes);
        }

        [HttpPost]
        public async Task<IActionResult> Spin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var result = await _spinService.SpinAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Json(result);
        }
    }
}
