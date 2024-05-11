
namespace Flipard.MVC.ViewModels
{
    public class HomeDeckViewModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CardCount { get; internal set; }
        public Guid Id { get; internal set; }
    }
}
