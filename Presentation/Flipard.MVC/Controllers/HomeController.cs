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

        public IActionResult Index()
        {
            return View();
        }

        [Authorize] // Sadece oturum açmýþ kullanýcýlar eriþebilir
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

            if (user == null)
            {
                return NotFound("Kullanýcý bulunamadý.");
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
            if(!ModelState.IsValid)
            {
                return View(homeCreateSetViewModel);
            }


            var cardId = Guid.NewGuid();
            

            var vocabulary = new Vocabulary
            {
                Id = Guid.NewGuid(),
                Term = homeCreateSetViewModel.Term,
                Meaning = homeCreateSetViewModel.Meaning,
                CardId = cardId,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,

            };

            var card = new Card
            {
                Id = cardId,
                Vocabulary = vocabulary,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            };

            
            var deck = new Deck
            {
                Id = Guid.NewGuid(),
                Name = homeCreateSetViewModel.Name,
                Description = homeCreateSetViewModel.Description,
                Cards = new List<Card> { card },
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            };

            _Appcontext.Vocabularies.Add(vocabulary);
            _Appcontext.Cards.Add(card);
            _Appcontext.Decks.Add(deck);

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
