using GameDataCollection.Models;

namespace GameDataCollection.ViewModels
{
    public class UserListViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Search { get; set; }
        public Dictionary<string, int> SpinsRemainingByUserId { get; set; } = new();

        /// <summary>Ids of non-admin accounts, the only ones an admin may reset a password for.</summary>
        public HashSet<string> ResettableUserIds { get; set; } = new();
    }
}
