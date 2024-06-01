using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class GameRecord
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public string? RefferedBy { get; set; }
        public required string Email { get; set; }
        public string? FacebookName { get; set; }
        public string? GameUserId { get; set; }
        public int StateId { get; set; }
        public int GameId { get; set; }
        [ForeignKey(nameof(StateId))]
        public required virtual State State { get; set; }
        [ForeignKey(nameof(GameId))]
        public required virtual Game Game { get; set; }
    }
}
