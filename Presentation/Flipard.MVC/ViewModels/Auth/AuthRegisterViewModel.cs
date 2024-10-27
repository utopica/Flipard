using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels.Auth;

public class AuthRegisterViewModel
{
    [Required] public string Email { get; set; }
    [Required] public string Username { get; set; }
    [Required] [MinLength(6)] public string Password { get; set; }
    public DateTimeOffset? Birthdate { get; set; }
}