using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class SpinHistory
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string PrizeLabel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrizeAmount { get; set; }

        public int SegmentIndex { get; set; }

        public DateTime SpunAt { get; set; } = DateTime.UtcNow;

        /// <summary>True when this spin consumed an admin-granted spin (does not reset the free-spin cooldown).</summary>
        public bool IsGrantedSpin { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
