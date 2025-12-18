using GameDataCollection.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameDataCollection.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class SpinController : Controller
    {
        private readonly ISpinService _spinService;

        public SpinController(ISpinService spinService)
        {
            _spinService = spinService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Spin()
        {
            // Replace this with however you get user id (Identity, custom, etc.)
            string userId = User.Identity?.Name ?? "anonymous";

            var result = await _spinService.SpinAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Json(result);
        }
    }
}
