using GameDataCollection.Models;
using System.Net;
using System.Text;

namespace GameDataCollection.Extension
{
    /// <summary>
    /// Email copies of chat messages. Every send runs in the background so a slow
    /// mail server never holds up the chat.
    /// </summary>
    public static class ChatMailer
    {
        public const string NotificationEmail = "aliyahhafeez29@gmail.com";

        private static readonly TimeZoneInfo NepalTz =
            TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

        /// <summary>Copies a message sent by a user to the notification address.</summary>
        public static void QueueUserMessageCopy(ChatMessage chatMessage, User? sender, string baseUrl)
        {
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
                body.Append($"<p><a href='{WebUtility.HtmlEncode(baseUrl + chatMessage.ImagePath)}'>View image</a></p>");

            var subject = $"New chat message from {senderName}";
            var html = body.ToString();

            Queue(NotificationEmail, subject, html);
        }

        /// <summary>Sends a message from the admin to the user's registered email.</summary>
        public static void QueueAdminMessageToUser(ChatMessage chatMessage, User? receiver, string baseUrl)
        {
            if (string.IsNullOrEmpty(receiver?.Email)) return;

            var body = EmailSender.AdminMessageTemplate(
                receiver.FullName ?? receiver.UserName ?? "there",
                chatMessage.Message,
                string.IsNullOrEmpty(chatMessage.ImagePath) ? null : baseUrl + chatMessage.ImagePath,
                baseUrl + "/Chat/Index");

            Queue(receiver.Email, "New message from Aliyah Gaming", body);
        }

        private static void Queue(string to, string subject, string body)
        {
            _ = Task.Run(() => EmailSender.EmailSend(to, subject, body));
        }
    }
}
