using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.ViewModels.Home;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "current password")]
    public string CurrentPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "new password")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "confirm new password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}