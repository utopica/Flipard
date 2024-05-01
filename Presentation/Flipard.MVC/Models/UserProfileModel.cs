using System.ComponentModel.DataAnnotations;

namespace Flipard.MVC.Models
{
    public class UserProfileModel
    {
        public string Email { get; set; }
       
        public string Username { get; set; }
        
        public string Password { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
    }
}
