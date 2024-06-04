using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class GameRecord
    {
        public GameRecord()
        {
            CreatedDateTime = DateTime.Now;
            ExpiryDateTime = CreatedDateTime.AddMonths(1);
        }

        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public string? RefferedBy { get; set; }
        public required string Email { get; set; }
        public string? FacebookName { get; set; }
        public string? GameUserId { get; set; }
        public int StateId { get; set; }
        public long GameId { get; set; }
        public DateTime CreatedDateTime { get; protected set; }
        public DateTime ExpiryDateTime { get; protected set; }

        [ForeignKey(nameof(StateId))]
        public virtual State? State { get; set; }
        [ForeignKey(nameof(GameId))]
        public virtual Game? Game { get; set; }
    }
}
