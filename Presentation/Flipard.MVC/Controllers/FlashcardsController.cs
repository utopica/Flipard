using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using Flipard.Domain.Entities;
using Flipard.Domain.Identity;
using Flipard.MVC.ViewModels.Flashcards;
using Flipard.MVC.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

[Authorize]
public class FlashcardsController : Controller
{
    private readonly ApplicationDbContext _appContext;
    private readonly UserManager<User> _userManager;

    public FlashcardsController(ApplicationDbContext appContext, UserManager<User> userManager)
    {
        _appContext = appContext;
        _userManager = userManager;
    }

    public IActionResult Index(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var deck = _appContext.Decks
            .Include(d => d.Cards)
            .ThenInclude(c => c.Vocabulary)
            .FirstOrDefault(d => d.Id == id);

        if (deck == null)
        {
            return NotFound();
        }

        bool isReadOnly = deck.CreatedByUserId != userId;

        var model = new HomeCreateSetViewModel
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description,
            TermMeanings = deck.Cards.Select(c => new TermMeaningViewModel
            {
                Id = c.Vocabulary.Id,
                Term = c.Vocabulary.Term,
                Meaning = c.Vocabulary.Meaning,
                ImageUrl = c.ImageUrl,
                IsReadOnly = isReadOnly ,
            }).ToList(),
            IsReadOnly = isReadOnly
        };

        return View(model);
    }

    [HttpDelete]
    public IActionResult DeleteCard(Guid id)
    {
        var card = _appContext.Cards.FirstOrDefault(c => c.Vocabulary.Id == id);

        if (card == null)
        {
            return NotFound();
        }

        _appContext.Cards.Remove(card);
        _appContext.SaveChanges();

        return Ok();
    }

    [HttpDelete]
    public IActionResult DeleteDeck(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var deck = _appContext.Decks.FirstOrDefault(d => d.Id == id && d.CreatedByUserId == userId);

        if (deck == null)
        {
            return NotFound();
        }

        _appContext.Decks.Remove(deck);
        _appContext.SaveChanges();

        return RedirectToAction(nameof(Index), "Home");
    }

    [HttpPut]
    public IActionResult UpdateCard([FromBody] TermMeaningViewModel updatedCard)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (updatedCard == null || updatedCard.Id == Guid.Empty)
        {
            return BadRequest("Invalid card data.");
        }

        var card = _appContext.Cards.Include(c => c.Vocabulary).FirstOrDefault(c => c.Vocabulary.Id == updatedCard.Id);

        if (card == null)
        {
            return NotFound();
        }
        
        if (!string.Equals(card.CreatedByUserId.Trim(), userId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized();
        }


        card.Vocabulary.Term = updatedCard.Term;
        card.Vocabulary.Meaning = updatedCard.Meaning;

        _appContext.SaveChanges();

        return Ok();
    }

    [HttpGet]
    public IActionResult CreateQuiz(Guid id)
    {
        var deck = _appContext.Decks
            .Include(d => d.Cards)
            .ThenInclude(c => c.Vocabulary)
            .FirstOrDefault(d => d.Id == id);

        if (deck == null)
        {
            return NotFound();
        }

        var model = new HomeCreateSetViewModel
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description,
            TermMeanings = deck.Cards.Select(c => new TermMeaningViewModel
            {
                Id = c.Vocabulary.Id,
                Term = c.Vocabulary.Term,
                Meaning = c.Vocabulary.Meaning,
                ImageUrl = c.ImageUrl,
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> QuizResults([FromBody] QuizResultViewModel results)
    {
        if (results is null) return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var quizAttemptId = Guid.NewGuid();

        var quizAttempt = new QuizAttempt
        {
            Id = quizAttemptId,
            DeckId = results.DeckId,
            UserId = user.Id,
            AttemptDate = DateTime.UtcNow,
            TotalQuestions = results.TotalQuestions,
            CorrectAnswers = results.CorrectAnswers,
            Accuracy = (double)results.CorrectAnswers / results.TotalQuestions * 100,
            TimeTakenSeconds = (int)(results.TimeTaken / 1000)
        };

        quizAttempt.Answers = results.AnswerDetails.Select(detail => new QuizAnswer
        {
            Id = Guid.NewGuid(),
            QuizAttemptId = quizAttemptId,
            VocabularyId = detail.VocabularyId,
            UserAnswer = detail.UserAnswer,
            IsCorrect = detail.IsCorrect,
        }).ToList();

        await _appContext.QuizAttempts.AddAsync(quizAttempt);
        await _appContext.SaveChangesAsync();

        return Json(new
            { success = true, redirectUrl = Url.Action("ShowStatistics", new { deckId = quizAttempt.DeckId }) });
    }


    [HttpGet]
    public async Task<IActionResult> ShowStatistics(Guid deckId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var deck = await _appContext.Decks
            .FirstOrDefaultAsync(d => d.Id == deckId);
        if (deck == null) return NotFound();

        var attempts = await _appContext.QuizAttempts
            .Include(qa => qa.Answers)
            .Where(qa => qa.DeckId == deckId && qa.UserId == user.Id)
            .OrderByDescending(qa => qa.AttemptDate)
            .ToListAsync();

        var mistakesByTerm = await _appContext.QuizAnswers
            .Include(qa => qa.Vocabulary)
            .Where(qa => qa.QuizAttempt.DeckId == deckId &&
                         qa.QuizAttempt.UserId == user.Id &&
                         !qa.IsCorrect)
            .GroupBy(qa => qa.Vocabulary.Term)
            .Select(g => new { Term = g.Key, MistakeCount = g.Count() })
            .OrderByDescending(x => x.MistakeCount)
            .Where(x => x.MistakeCount > 2) 
            .ToDictionaryAsync(x => x.Term, x => x.MistakeCount);

        var statistics = new QuizStatisticsViewModel
        {
            DeckName = deck.Name,
            TotalAttempts = attempts.Count,
            AverageAccuracy = attempts.Any() ? attempts.Average(a => a.Accuracy) : 0,
            BestAccuracy = attempts.Any() ? attempts.Max(a => a.Accuracy) : 0,
            TotalCorrectAnswers = attempts.Sum(a => a.CorrectAnswers),
            TotalQuestions = attempts.Sum(a => a.TotalQuestions),
            MostMistakenTerms = mistakesByTerm,
            RecentAttempts = attempts.Take(5)
                .Select(a => new QuizAttemptSummary
                {
                    AttemptDate = a.AttemptDate,
                    CorrectAnswers = a.CorrectAnswers,
                    TotalQuestions = a.TotalQuestions,
                    Accuracy = a.Accuracy,
                    TimeTakenSeconds = a.TimeTakenSeconds
                }).ToList()
        };

        return View(statistics);
    }

    [HttpGet]
    public IActionResult EditSet(Guid id)
    {
        var deck = _appContext.Decks
            .Include(d => d.Cards)
            .ThenInclude(c => c.Vocabulary)
            .FirstOrDefault(d => d.Id == id);

        if (deck == null)
        {
            return NotFound();
        }

        var model = new HomeCreateSetViewModel
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description,
            TermMeanings = deck.Cards.Select(c => new TermMeaningViewModel
            {
                Id = c.Vocabulary.Id,
                Term = c.Vocabulary.Term,
                Meaning = c.Vocabulary.Meaning,
                ImageUrl = c.ImageUrl,
            }).ToList(),
            IsReadOnly = false
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult EditSet(HomeCreateSetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var deck = _appContext.Decks
            .Include(d => d.Cards)
            .ThenInclude(c => c.Vocabulary)
            .FirstOrDefault(d => d.Id == model.Id);

        if (deck == null)
        {
            return NotFound();
        }

        deck.Name = model.Name;
        deck.Description = model.Description;

        foreach (var termMeaning in model.TermMeanings)
        {
            if (termMeaning.Id != Guid.Empty)
            {
                var card = deck.Cards.FirstOrDefault(c => c.Vocabulary.Id == termMeaning.Id);
                if (card != null)
                {
                    card.Vocabulary.Term = termMeaning.Term;
                    card.Vocabulary.Meaning = termMeaning.Meaning;
                    card.ImageUrl = termMeaning.ImageUrl;
                }
            }
            else
            {
                var card = new Card
                {
                    Id = Guid.NewGuid(),
                    DeckId = deck.Id,
                    Deck = deck,
                    ImageUrl = termMeaning.ImageUrl,
                    CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                var vocabulary = new Vocabulary
                {
                    Id = Guid.NewGuid(),
                    Term = termMeaning.Term,
                    Meaning = termMeaning.Meaning,
                    Card = card,
                    CardId = card.Id,
                    CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                };

                card.Vocabulary = vocabulary;

                _appContext.Cards.Add(card);
                _appContext.Vocabularies.Add(vocabulary);
                deck.Cards.Add(card);
            }
        }

        _appContext.SaveChanges();
        return RedirectToAction("Index", new { id = deck.Id });
    }
}