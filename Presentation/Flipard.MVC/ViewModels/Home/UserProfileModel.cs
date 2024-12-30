using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels.Home
{
    public class UserProfileModel
    {
        [Required] [EmailAddress] public string Email { get; set; }

        [Required] public string Username { get; set; }

        public string Password { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }
}