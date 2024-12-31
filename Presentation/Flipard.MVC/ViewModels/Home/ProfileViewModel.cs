namespace Flipard.MVC.ViewModels.Home;

public class ProfileViewModel
{
    public string? Username { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime JoinedDate { get; set; }
    public int CurrentLevel { get; set; }
    public int CurrentXP { get; set; }
    public int RequiredXP { get; set; }
    public int BadgeCount { get; set; }
}