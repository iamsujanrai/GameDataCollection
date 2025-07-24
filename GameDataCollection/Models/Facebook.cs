using System.ComponentModel.DataAnnotations;

namespace GameDataCollection.Models
{
    public class Facebook
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
