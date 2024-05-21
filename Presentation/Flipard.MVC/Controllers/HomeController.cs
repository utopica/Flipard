using Flipard.Domain.Entities;
using Flipard.MVC.Models;
using Flipard.MVC.ViewModels;
using Flipard.Persistence.Contexts;
using Flipard.Persistence.Contexts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Flipard.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IdentityContext _context;
        private readonly ApplicationDbContext _Appcontext;
        public HomeController(ILogger<HomeController> logger, IdentityContext context, ApplicationDbContext appcontext)
        {
            _logger = logger;
            _context = context;
            _Appcontext = appcontext;
        }

        //public IActionResult Index()
        //{

        //    return View();
        //}

        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var decks = _Appcontext.Decks
                .Where(d => d.CreatedByUserId == userId)
                .Select(d => new HomeDeckDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CardCount = d.Cards.Count()
                })
                .ToList();

            return View(decks);
        }


        [Authorize] // Sadece oturum a�m�� kullan�c�lar eri�ebilir
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

            if (user == null)
            {
                return NotFound("Kullan bulunamad�.");
            }

            var userProfile = new UserProfileModel
            {
                Birthdate = user.Birthdate,
                Email = user.Email,
                Password = "******",
                Username = user.UserName

            };


            return View(userProfile);
        }


        [HttpGet]
        public IActionResult CreateSet()
        {
            var createSetViewModel = new HomeCreateSetViewModel();
            return View(createSetViewModel);
        }

        [HttpPost]
        public IActionResult CreateSet(HomeCreateSetViewModel homeCreateSetViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(homeCreateSetViewModel);
            }

            var deck = _Appcontext.Decks.FirstOrDefault(d => d.Id == homeCreateSetViewModel.Id);

            if (deck == null)
            {
                // Create a new deck if it doesn't exist
                deck = new Deck
                {
                    Id = Guid.NewGuid(),
                    Name = homeCreateSetViewModel.Name,
                    Description = homeCreateSetViewModel.Description,
                    Cards = new List<Card>(),
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                _Appcontext.Decks.Add(deck);
            }

            foreach (var termMeaning in homeCreateSetViewModel.TermMeanings)
            {
                var cardId = Guid.NewGuid();

                var vocabulary = new Vocabulary
                {
                    Id = Guid.NewGuid(),
                    Term = termMeaning.Term,
                    Meaning = termMeaning.Meaning,
                    CardId = cardId,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                var card = new Card
                {
                    Id = cardId,
                    Vocabulary = vocabulary,
                    Deck = deck,
                    DeckId = deck.Id,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                deck.Cards.Add(card);

                _Appcontext.Vocabularies.Add(vocabulary);
                _Appcontext.Cards.Add(card);
            }

            _Appcontext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}