namespace Flipard.MVC.ViewModels
{
    public class HomePageViewModel
    {
        public IEnumerable<HomeDeckDetailsViewModel> UserDecks { get; set; }
        public IEnumerable<HomeDeckDetailsViewModel> RandomUserDecks { get; set; }
    }

}
