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
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeController(ILogger<HomeController> logger, IdentityContext context, ApplicationDbContext appcontext, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _Appcontext = appcontext;
            _hostingEnvironment = hostingEnvironment;
        }


        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userDecks = _Appcontext.Decks
                .Where(d => d.CreatedByUserId == userId)
                .Select(d => new HomeDeckDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CardCount = d.Cards.Count()
                })
                .ToList();

            var randomUserDecks = _Appcontext.Decks
                .Where(d => d.CreatedByUserId != userId)
                .OrderBy(r => Guid.NewGuid())
                .Select(d => new HomeDeckDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CardCount = d.Cards.Count()
                })
                .Take(5) //limit the number of random decks
                .ToList();

            var viewModel = new HomePageViewModel
            {
                UserDecks = userDecks,
                RandomUserDecks = randomUserDecks
            };

            return View(viewModel);
        }
        
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
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
        public async Task<IActionResult> CreateSet(HomeCreateSetViewModel homeCreateSetViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(homeCreateSetViewModel);
            }

            var deck = _Appcontext.Decks.FirstOrDefault(d => d.Id == homeCreateSetViewModel.Id);

            if (deck == null)
            {
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
                var imageUrl = "";

                if (termMeaning.Image != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    var filePath = Path.Combine(uploads, termMeaning.Image.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await termMeaning.Image.CopyToAsync(fileStream);
                    }

                    imageUrl = "/uploads/" + termMeaning.Image.FileName;
                }

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
                    ImageUrl = imageUrl,
                };

                deck.Cards.Add(card);

                _Appcontext.Vocabularies.Add(vocabulary);
                _Appcontext.Cards.Add(card);
            }

            _Appcontext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public JsonResult SearchDecks(string query)
        {
            var decks = _Appcontext.Decks 
                .Where(d => d.Name.Contains(query))
                .Select(d => new { d.Id, d.Name })
                .ToList();

            return Json(decks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}