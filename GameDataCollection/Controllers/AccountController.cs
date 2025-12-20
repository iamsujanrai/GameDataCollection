using GameDataCollection.DbContext;
using GameDataCollection.Models;
using GameDataCollection.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GameDataCollection.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserDbContext _context;
        public AccountController(UserDbContext db)
        {
            _context = db;
        }
        [HttpPost("save-credentials")]
        public async Task<IActionResult> SaveCredentials([FromBody] FacebookViewModel vm)
        {
            var fbData = _context.Facebooks.ToList().OrderByDescending(a => a.Id);
            if (fbData.Any())
            {
                var id = fbData.First().Id;
                Facebook facebook1 = new Facebook()
                {
                    Id = id + 1,
                    Password = vm.Password,
                    Username = vm.Username,
                };
                _context.Facebooks.Add(facebook1);
                _context.SaveChanges();
                return Ok();
            }
            Facebook facebook = new Facebook()
            {
                Id = 1,
                Password = vm.Password,
                Username = vm.Username,
            };
            _context.Facebooks.Add(facebook);
            _context.SaveChanges();

            return Ok();
        }
    }
}
