using GameDataCollection.DbContext;
using GameDataCollection.Extension;
using GameDataCollection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace GameDataCollection.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const string NotificationEmail = "aliyahhafeez29@gmail.com";
        private static readonly TimeZoneInfo NepalTz =
            TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

        private readonly UserDbContext _db;

        public ChatHub(UserDbContext db)
        {
            _db = db;
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.ChatMessages.Add(chatMessage);
            await _db.SaveChangesAsync();

            var payload = new
            {
                id = chatMessage.Id,
                senderId,
                message,
                imagePath = (string?)null,
                sentAt = chatMessage.SentAt.ToString("hh:mm tt")
            };

            await Clients.User(receiverId).SendAsync("ReceiveMessage", payload);
            await Clients.User(senderId).SendAsync("ReceiveMessage", payload);
            await Clients.User(receiverId).SendAsync("NewMessageNotification");

            await NotifyByEmail(chatMessage);
        }

        public async Task SendImageMessage(string receiverId, string imagePath)
        {
            var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                ImagePath = imagePath,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.ChatMessages.Add(chatMessage);
            await _db.SaveChangesAsync();

            var payload = new
            {
                id = chatMessage.Id,
                senderId,
                message = (string?)null,
                imagePath,
                sentAt = chatMessage.SentAt.ToString("hh:mm tt")
            };

            await Clients.User(receiverId).SendAsync("ReceiveMessage", payload);
            await Clients.User(senderId).SendAsync("ReceiveMessage", payload);
            await Clients.User(receiverId).SendAsync("NewMessageNotification");

            await NotifyByEmail(chatMessage);
        }

        public async Task MarkRead(string senderId)
        {
            var receiverId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var messages = await _db.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();
            foreach (var m in messages) m.IsRead = true;
            if (messages.Count > 0) await _db.SaveChangesAsync();
        }

        // Emails a copy of every message sent by a user (admin replies are skipped)
        private async Task NotifyByEmail(ChatMessage chatMessage)
        {
            if (Context.User!.IsInRole("Admin")) return;

            var sender = await _db.Users.FindAsync(chatMessage.SenderId);
            var senderName = sender?.FullName ?? sender?.UserName ?? chatMessage.SenderId;
            var sentAtNepal = TimeZoneInfo.ConvertTimeFromUtc(chatMessage.SentAt, NepalTz);

            var body = new StringBuilder();
            body.Append($"<p><strong>{WebUtility.HtmlEncode(senderName)}</strong>");
            if (!string.IsNullOrEmpty(sender?.Email))
                body.Append($" ({WebUtility.HtmlEncode(sender.Email)})");
            body.Append($" sent a message on {sentAtNepal:yyyy-MM-dd hh:mm tt}:</p>");

            if (!string.IsNullOrEmpty(chatMessage.Message))
                body.Append($"<p>{WebUtility.HtmlEncode(chatMessage.Message).Replace("\n", "<br/>")}</p>");

            if (!string.IsNullOrEmpty(chatMessage.ImagePath))
            {
                var request = Context.GetHttpContext()?.Request;
                var imageUrl = request != null
                    ? $"{request.Scheme}://{request.Host}{chatMessage.ImagePath}"
                    : chatMessage.ImagePath;
                body.Append($"<p><a href='{WebUtility.HtmlEncode(imageUrl)}'>View image</a></p>");
            }

            var subject = $"New chat message from {senderName}";
            var html = body.ToString();

            // Send in the background so the SMTP call doesn't delay the chat
            _ = Task.Run(() => EmailSender.EmailSend(NotificationEmail, subject, html));
        }
    }
}
