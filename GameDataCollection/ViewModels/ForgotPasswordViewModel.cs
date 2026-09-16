using System.ComponentModel.DataAnnotations;

namespace GameDataCollection.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
