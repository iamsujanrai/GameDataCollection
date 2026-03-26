using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class SpinPrize
    {
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int Weight { get; set; } = 10;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
