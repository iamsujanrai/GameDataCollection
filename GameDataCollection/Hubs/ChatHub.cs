using GameDataCollection.DbContext;
using GameDataCollection.Extension;
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

        // A message from the admin goes to the user's registered email;
        // a message from a user is copied to the notification address.
        private async Task NotifyByEmail(ChatMessage chatMessage)
        {
            if (Context.User!.IsInRole("Admin"))
            {
                var receiver = await _db.Users.FindAsync(chatMessage.ReceiverId);
                ChatMailer.QueueAdminMessageToUser(chatMessage, receiver, BaseUrl());
                return;
            }

            var sender = await _db.Users.FindAsync(chatMessage.SenderId);
            ChatMailer.QueueUserMessageCopy(chatMessage, sender, BaseUrl());
        }

        private string BaseUrl()
        {
            var request = Context.GetHttpContext()?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : string.Empty;
        }
    }
}
