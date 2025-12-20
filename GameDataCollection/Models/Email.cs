using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameDataCollection.Models
{
    [Table("Emails")]
    public class Email
    {
        public Email()
        {
            IsActive = true;
        }
        [Key]
        public long Id { get; set; }
        public required string MemberEmail { get; set; }
        public bool IsActive { get; set; }
    }
}
