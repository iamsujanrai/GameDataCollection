using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    public class UserSpinGrant
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int SpinsGranted { get; set; }
        public int SpinsUsed { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }
        public string? GrantedByAdminId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
