using GameDataCollection.DbContext;
using GameDataCollection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameDataCollection.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
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
    }
}
