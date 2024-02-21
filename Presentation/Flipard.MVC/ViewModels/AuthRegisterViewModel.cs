using Flipard.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels
{
    public class AuthRegisterViewModel
    {
        //what i need to register an user.
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
        public Gender Gender { get; set; }

    }
}
