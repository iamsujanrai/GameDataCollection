using GameDataCollection.Models;

namespace GameDataCollection.ViewModels
{
    public class UserDashboardViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int SpinsRemainingToday { get; set; }
        public int FreeSpinsRemaining { get; set; }
        public int GrantedSpinsRemaining { get; set; }
        public DateTime? NextSpinAvailableAt { get; set; }
        public GameRecord? GameRecord { get; set; }
        public List<SpinHistory> SpinHistory { get; set; } = new();

        /// <summary>Who the dashboard message card talks to; null when no admin account exists.</summary>
        public string? AdminId { get; set; }
        public string? UserId { get; set; }
        public List<ChatMessage> RecentMessages { get; set; } = new();
    }
}
