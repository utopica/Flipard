using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels.Home
{
    public class AuthEditProfileViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTimeOffset? Birthdate { get; set; }
    }
}