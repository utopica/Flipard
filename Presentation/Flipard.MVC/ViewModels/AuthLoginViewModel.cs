using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels;

public class AuthLoginViewModel
{
    [Required] public string UserIdentifier { get; set; }
    [Required] public string Password { get; set; }
}