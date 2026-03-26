using GameDataCollection.Models;
using System.ComponentModel.DataAnnotations;

namespace GameDataCollection.ViewModels
{
    public class GrantSpinsViewModel
    {
        [Required]
        [Display(Name = "User")]
        public string UserId { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Number of Spins")]
        public int SpinsToGrant { get; set; } = 1;

        [Display(Name = "Note (optional)")]
        public string? Note { get; set; }

        // For populating the user dropdown
        public List<User> Users { get; set; } = new();
    }
}
