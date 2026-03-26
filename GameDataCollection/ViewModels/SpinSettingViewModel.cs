using System.ComponentModel.DataAnnotations;

namespace GameDataCollection.ViewModels
{
    public class SpinSettingViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 720, ErrorMessage = "Cooldown must be between 1 and 720 hours.")]
        [Display(Name = "Cooldown (hours)")]
        public int CooldownHours { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Max spins must be between 1 and 100.")]
        [Display(Name = "Max Spins per Period")]
        public int MaxSpinsPerPeriod { get; set; }
    }
}
