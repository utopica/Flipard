﻿namespace Flipard.MVC.ViewModels.Home;

public class HomePageViewModel
{
    public IEnumerable<HomeDeckDetailsViewModel> UserDecks { get; set; }
    public IEnumerable<HomeDeckDetailsViewModel> RandomUserDecks { get; set; }
    public ProfileViewModel UserProfile { get; set; }
    public CardViewModel QuestionOfTheDay { get; set; }
    public IEnumerable<RecentlyStudiedDeck> RecentlyStudiedDeck { get; set; }
}