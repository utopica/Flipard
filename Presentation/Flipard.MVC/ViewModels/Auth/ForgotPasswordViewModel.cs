using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}