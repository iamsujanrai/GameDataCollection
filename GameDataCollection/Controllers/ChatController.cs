using GameDataCollection.DbContext;
using GameDataCollection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GameDataCollection.Hubs;

namespace GameDataCollection.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(UserDbContext db, UserManager<User> userManager, IWebHostEnvironment env, IHubContext<ChatHub> hub)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _hub = hub;
        }

        // ── User chat with admin ──────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminId = admins.FirstOrDefault()?.Id;
            if (adminId == null) return NotFound("No admin account found.");

            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == adminId) ||
                             (m.SenderId == adminId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Mark incoming messages as read
            var unread = messages.Where(m => m.SenderId == adminId && !m.IsRead).ToList();
            foreach (var msg in unread) msg.IsRead = true;
            if (unread.Count > 0) await _db.SaveChangesAsync();

            ViewBag.AdminId = adminId;
            ViewBag.UserId = userId;
            return View(messages);
        }

        // ── Admin chat ────────────────────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChat(string? userId = null)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            // Users who have ever messaged admin (or admin messaged them)
            var chattedUserIds = await _db.ChatMessages
                .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                .Select(m => m.SenderId == adminId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var chattedUsers = await _userManager.Users
                .Where(u => chattedUserIds.Contains(u.Id) && u.Id != adminId)
                .ToListAsync();

            // Unread counts per user for admin
            var unreadByUser = await _db.ChatMessages
                .Where(m => m.ReceiverId == adminId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            List<ChatMessage> messages = new();
            if (userId != null)
            {
                messages = await _db.ChatMessages
                    .Where(m => (m.SenderId == adminId && m.ReceiverId == userId) ||
                                 (m.SenderId == userId && m.ReceiverId == adminId))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                // Mark incoming messages as read
                var unread = messages.Where(m => m.SenderId == userId && !m.IsRead).ToList();
                foreach (var msg in unread) msg.IsRead = true;
                if (unread.Count > 0) await _db.SaveChangesAsync();
            }

            // All users for "new conversation" dropdown
            var allUsers = await _userManager.Users
                .Where(u => u.Id != adminId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.AdminId = adminId;
            ViewBag.SelectedUserId = userId;
            ViewBag.ChattedUsers = chattedUsers;
            ViewBag.UnreadByUser = unreadByUser;
            ViewBag.AllUsers = allUsers;
            return View("~/Views/Admin/Chat.cshtml", messages);
        }

        // ── Unread count (JSON) ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var count = await _db.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
            return Json(count);
        }

        // ── Bulk message ──────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkMessage()
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var users = await _userManager.Users
                .Where(u => u.Id != adminId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return View("~/Views/Admin/BulkMessage.cshtml", users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkMessage(List<string> userIds, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(BulkMessage));
            }
            if (userIds == null || userIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one user.";
                return RedirectToAction(nameof(BulkMessage));
            }

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var sentAt = DateTime.UtcNow;

            var chatMessages = userIds.Select(uid => new ChatMessage
            {
                SenderId = adminId,
                ReceiverId = uid,
                Message = message,
                SentAt = sentAt,
                IsRead = false
            }).ToList();

            _db.ChatMessages.AddRange(chatMessages);
            await _db.SaveChangesAsync();

            // Push real-time notifications to online users
            var payload = new
            {
                senderId = adminId,
                message,
                imagePath = (string?)null,
                sentAt = sentAt.ToLocalTime().ToString("hh:mm tt")
            };

            foreach (var uid in userIds)
            {
                await _hub.Clients.User(uid).SendAsync("ReceiveMessage", payload);
                await _hub.Clients.User(uid).SendAsync("NewMessageNotification");
            }

            TempData["Success"] = $"Message sent to {userIds.Count} user(s).";
            return RedirectToAction(nameof(BulkMessage));
        }

        // ── Image upload ──────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file.");

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest("Only image files are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Max file size is 5 MB.");

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "chat");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Json(new { path = $"/uploads/chat/{fileName}" });
        }
    }
}
