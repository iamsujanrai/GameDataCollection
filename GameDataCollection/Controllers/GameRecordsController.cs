using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameDataCollection.DbContext;
using GameDataCollection.Models;

namespace GameDataCollection.Controllers
{
    public class GameRecordsController : Controller
    {
        private readonly UserDbContext _context;

        public GameRecordsController(UserDbContext context)
        {
            _context = context;
        }

        // GET: GameRecords
        public async Task<IActionResult> Index()
        {
            var userDbContext = _context.GamesRecord.Include(g => g.Game).Include(g => g.State);
            return View(await userDbContext.ToListAsync());
        }

        // GET: GameRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameRecord = await _context.GamesRecord
                .Include(g => g.Game)
                .Include(g => g.State)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameRecord == null)
            {
                return NotFound();
            }

            return View(gameRecord);
        }

        // GET: GameRecords/Create
        public IActionResult Create()
        {
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Name");
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name");
            return View();
        }

        // POST: GameRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,PhoneNumber,RefferedBy,Email,FacebookName,GameUserId,StateId,GameId")] GameRecord gameRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gameRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", gameRecord.GameId);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Id", gameRecord.StateId);
            return View(gameRecord);
        }

        // GET: GameRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameRecord = await _context.GamesRecord.FindAsync(id);
            if (gameRecord == null)
            {
                return NotFound();
            }
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", gameRecord.GameId);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Id", gameRecord.StateId);
            return View(gameRecord);
        }

        // POST: GameRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,RefferedBy,Email,FacebookName,GameUserId,StateId,GameId")] GameRecord gameRecord)
        {
            if (id != gameRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameRecordExists(gameRecord.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Id", gameRecord.GameId);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Id", gameRecord.StateId);
            return View(gameRecord);
        }

        // GET: GameRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameRecord = await _context.GamesRecord
                .Include(g => g.Game)
                .Include(g => g.State)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameRecord == null)
            {
                return NotFound();
            }

            return View(gameRecord);
        }

        // POST: GameRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gameRecord = await _context.GamesRecord.FindAsync(id);
            if (gameRecord != null)
            {
                _context.GamesRecord.Remove(gameRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameRecordExists(int id)
        {
            return _context.GamesRecord.Any(e => e.Id == id);
        }
    }
}
