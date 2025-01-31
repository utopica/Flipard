﻿using Flipard.Domain.Entities;
using Flipard.MVC.Models;
using Flipard.MVC.ViewModels;
using Flipard.Persistence.Contexts;
using Flipard.Persistence.Contexts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Flipard.Domain.Identity;
using Flipard.Domain.Interfaces;
using Flipard.MVC.ViewModels.Auth;
using Flipard.MVC.ViewModels.Home;
using Flipard.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Flipard.MVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IdentityContext _context;
    private readonly ApplicationDbContext _appcontext;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly INToastNotifyService _nToastNotifyService;

    public HomeController(ILogger<HomeController> logger, IdentityContext context, ApplicationDbContext appcontext,
        IWebHostEnvironment hostingEnvironment, IEmailService emailService, UserManager<User> userManager,
        INToastNotifyService nToastNotifyService)
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
        var user = _context.Users.FirstOrDefault(x => x.Id == Guid.Parse(userId));

        if (user is null)
        {
            return Unauthorized();
        }

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
            .Take(5)
            .ToList();

        var totalQuizAttempts = _appcontext.QuizAttempts
            .Count(qa => qa.UserId == user.Id);
    
        var currentXp = totalQuizAttempts;
        var currentLevel = (currentXp / 50) + 1; 
        var requiredXp = currentLevel * 50; 

        
        var userLevel = _appcontext.UserLevels.FirstOrDefault(x => x.UserId == user.Id);

        if (userLevel == null)
        {
            userLevel = new UserLevel
            {
                UserId = user.Id,
                Level = currentLevel,
                CurrentExperience = currentXp,
                RequiredExperience = requiredXp,
                CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            _appcontext.UserLevels.Add(userLevel);
        }
        else
        {
            userLevel.Level = currentLevel;
            userLevel.CurrentExperience = currentXp;
            userLevel.RequiredExperience = requiredXp;
            _appcontext.UserLevels.Update(userLevel);
        }
    
        _appcontext.SaveChanges();
        
        var badgeCount = _appcontext.UserBadges.Count(b => b.UserId == user.Id);

        var userProfile = new ProfileViewModel()
        {
            Username = user.UserName,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            JoinedDate = user.CreatedOn.DateTime,
            BadgeCount = badgeCount,
            CurrentLevel = userLevel?.Level ?? 1,  
            CurrentXP = userLevel?.CurrentExperience ?? 50, 
            RequiredXP = userLevel?.RequiredExperience ?? 100, 
        };

        var randomCard = _appcontext.Decks
            .Where(d => d.CreatedByUserId == userId)
            .SelectMany(d => d.Cards)
            .OrderBy(r => Guid.NewGuid())
            .Select(v => v.Vocabulary)
            .Select(c => new CardViewModel
            {
                Term = c.Term,
                Meaning = c.Meaning
            })
            .FirstOrDefault();
        
        if (randomCard == null)
        {
            randomCard = new CardViewModel
            {
                Term = "Create your first card!",
                Meaning = "Start by adding cards to your decks"
            };
        }

        var quizAttempt = _appcontext.QuizAttempts
            .Include(x => x.Deck) 
            .ThenInclude(deck => deck.Cards) 
            .Where(x => true) 
            .GroupBy(x => new { x.DeckId, x.Deck.Name }) 
            .Select(g => new RecentlyStudiedDeck
            {
                Id = g.Key.DeckId,
                Name = g.Key.Name,
                CardCount = g.First().Deck.Cards.Count, 
                LastAttemptDate = g.Max(attempt => attempt.AttemptDate) 
            })
            .OrderByDescending(deck => deck.LastAttemptDate) 
            .ToList(); 

        var viewModel = new HomePageViewModel
        {
            UserDecks = userDecks,
            RandomUserDecks = randomUserDecks,
            UserProfile = userProfile,
            QuestionOfTheDay = randomCard,
            RecentlyStudiedDeck = quizAttempt,
        };

        return View(viewModel);
    }

    public IActionResult Profile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID is null or empty.");
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var userProfile = new UserProfileModel
        {
            Birthdate = user.Birthdate,
            Email = user.Email,
            Password = "******",
            Username = user.UserName,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };

        return View(userProfile);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        
        var editProfileViewModel = new AuthEditProfileViewModel
        {
            Username = user!.UserName!,
            Email = user.Email!,
            Birthdate = user.Birthdate?.ToLocalTime(),
            ProfilePhotoUrl = user.ProfilePhotoUrl,
        };

        return View(editProfileViewModel);
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> EditProfileAsync(AuthEditProfileViewModel editProfileViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(editProfileViewModel);
        }

        var user = await _userManager.GetUserAsync(User);
    
        user!.UserName = editProfileViewModel.Username;
        user.Email = editProfileViewModel.Email;
        if (editProfileViewModel.Birthdate != null)
            user.Birthdate = editProfileViewModel.Birthdate.Value.UtcDateTime;
                
    
        if (editProfileViewModel.ProfilePhoto != null)
        {
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var fileName = $"{Path.GetFileNameWithoutExtension(editProfileViewModel.ProfilePhoto.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(editProfileViewModel.ProfilePhoto.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await editProfileViewModel.ProfilePhoto.CopyToAsync(fileStream);
            }

            user.ProfilePhotoUrl = "/uploads/" + fileName;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(editProfileViewModel);
        }

        _nToastNotifyService.AddSuccessToastMessage("Your profile has been updated successfully.");
    
        editProfileViewModel.ProfilePhotoUrl = user.ProfilePhotoUrl;
    
        return RedirectToAction("EditProfile");
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
        if (user != null){
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
            var encodedToken = WebUtility.UrlEncode(token);
        
            var callbackUrl = Url.Action(
                "ResetPassword", 
                "Home", 
                new { email = model.Email, token = encodedToken },
                protocol: HttpContext.Request.Scheme
            );

            await _emailService.SendEmailAsync(
                model.Email, 
                "flipardapp@gmail.com",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a>"
            );
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new ResetPasswordViewModel 
        { 
            Email = email,
            Token = token 
        };
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
            return RedirectToAction("Login", "Auth");
        }

        var decodedToken = WebUtility.UrlDecode(model.Token);
    
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction("Login", "Auth");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }


    [HttpGet]
    public IActionResult CreateSet()
    {
        var createSetViewModel = new HomeCreateSetViewModel
        {
            TermMeanings = new List<TermMeaningViewModel>() 
        };
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