using GameDataCollection.Models;

namespace GameDataCollection.ViewModels
{
    public class UserDashboardViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int SpinsRemainingToday { get; set; }
        public GameRecord? GameRecord { get; set; }
        public List<SpinHistory> SpinHistory { get; set; } = new();
    }
}
