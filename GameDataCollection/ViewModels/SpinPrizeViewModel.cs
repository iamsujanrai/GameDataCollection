using System.ComponentModel.DataAnnotations;

namespace GameDataCollection.ViewModels
{
    public class SpinPrizeViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Label")]
        public string Label { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Display(Name = "Amount ($)")]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Weight must be between 1 and 1000.")]
        [Display(Name = "Weight (higher = more likely)")]
        public int Weight { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
