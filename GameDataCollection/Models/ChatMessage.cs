using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class ChatMessage
    {
        public long Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public string? Message { get; set; }

        public string? ImagePath { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual User? Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual User? Receiver { get; set; }
    }
}
