using Flipard.Domain.Entities;
using Flipard.MVC.Models;
using Flipard.MVC.ViewModels;
using Flipard.Persistence.Contexts;
using Flipard.Persistence.Contexts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Flipard.Domain.Identity;
using Flipard.Domain.Interfaces;
using Flipard.Persistence.Services;
using Microsoft.AspNetCore.Identity;

namespace Flipard.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IdentityContext _context;
        private readonly ApplicationDbContext _appcontext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly INToastNotifyService _nToastNotifyService;
        
        public HomeController(ILogger<HomeController> logger, IdentityContext context, ApplicationDbContext appcontext, IWebHostEnvironment hostingEnvironment, IEmailService emailService, UserManager<User> userManager, INToastNotifyService nToastNotifyService)
        {
            _logger = logger;
            _context = context;
            _appcontext = appcontext;
            _hostingEnvironment = hostingEnvironment;
            _emailService = emailService;
            _userManager = userManager;
            _nToastNotifyService = nToastNotifyService;
        }


        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userDecks = _appcontext.Decks
                .Where(d => d.CreatedByUserId == userId)
                .Select(d => new HomeDeckDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CardCount = d.Cards.Count()
                })
                .ToList();

            var randomUserDecks = _appcontext.Decks
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
        
        public IActionResult ChangePassword()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                
                _nToastNotifyService.AddSuccessToastMessage("Your password has been changed successfully.");

                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                
                _nToastNotifyService.AddErrorToastMessage("Your password hasn't been changed.");
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Home", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View(); //Ive created the view
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
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

            var deck = _appcontext.Decks.FirstOrDefault(d => d.Id == homeCreateSetViewModel.Id);

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

                _appcontext.Decks.Add(deck);
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

                _appcontext.Vocabularies.Add(vocabulary);
                _appcontext.Cards.Add(card);
            }

            _appcontext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public JsonResult SearchDecks(string query)
        {
            var decks = _appcontext.Decks 
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