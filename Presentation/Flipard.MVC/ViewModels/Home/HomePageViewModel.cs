namespace Flipard.MVC.ViewModels.Home
{
    public class HomePageViewModel
    {
        public IEnumerable<HomeDeckDetailsViewModel> UserDecks { get; set; }
        public IEnumerable<HomeDeckDetailsViewModel> RandomUserDecks { get; set; }
    }

}
