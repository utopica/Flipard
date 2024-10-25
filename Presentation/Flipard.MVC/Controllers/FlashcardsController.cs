using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Flipard.Domain.Entities;
using Flipard.Domain.Identity;
using Flipard.MVC.Models.Flashcards;
using Microsoft.AspNetCore.Identity;

public class FlashcardsController : Controller
{
    private readonly ApplicationDbContext _appContext;
    private readonly UserManager<User> _userManager;

    public FlashcardsController(ApplicationDbContext appContext, UserManager<User> userManager)
    {
        _appContext = appContext;
        _userManager = userManager;
    }

    public IActionResult Index(Guid id, bool isReadOnly = false)
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
        var deck = _appContext.Decks.FirstOrDefault(d => d.Id == id);

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
        if (updatedCard == null || updatedCard.Id == Guid.Empty)
        {
            return BadRequest("Invalid card data.");
        }

        var card = _appContext.Cards.Include(c => c.Vocabulary).
            FirstOrDefault(c => c.Vocabulary.Id == updatedCard.Id);

        if (card == null)
        {
            return NotFound();
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

        var quizAttempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            DeckId = results.DeckId,
            UserId = user.Id,
            AttemptDate = DateTime.UtcNow,
            TotalQuestions = results.TotalQuestions,
            CorrectAnswers = results.CorrectAnswers,
            Accuracy = (double)results.CorrectAnswers / results.TotalQuestions * 100,
            TimeTakenSeconds = (int)(results.TimeTaken / 1000), 
            Answers = results.AnswerDetails.Select(detail => new QuizAnswer
            {
                Id = Guid.NewGuid(),
                VocabularyId = detail.VocabularyId, //todo: Make sure to include this in your client-side model
                UserAnswer = detail.UserAnswer,
                IsCorrect = detail.IsCorrect
            }).ToList()
        };

        _appContext.QuizAttempts.Add(quizAttempt);
        await _appContext.SaveChangesAsync();

        return Ok(new { redirectUrl = Url.Action("ShowStatistics", new { deckId = results.DeckId }) });
    }
    
    [HttpGet]
    public async Task<IActionResult> ShowStatistics(Guid deckId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var deck = await _appContext.Decks
            .FirstOrDefaultAsync(d => d.Id == deckId);
        if (deck == null) return NotFound();

        // Get all attempts for this deck by this user
        var attempts = await _appContext.QuizAttempts
            .Include(qa => qa.Answers)
            .Where(qa => qa.DeckId == deckId && qa.UserId == user.Id)
            .OrderByDescending(qa => qa.AttemptDate)
            .ToListAsync();

        // Get most mistaken terms
        var mistakesByTerm = await _appContext.QuizAnswers
            .Include(qa => qa.Vocabulary)
            .Where(qa => qa.QuizAttempt.DeckId == deckId && 
                        qa.QuizAttempt.UserId == user.Id && 
                        !qa.IsCorrect)
            .GroupBy(qa => qa.Vocabulary.Term)
            .Select(g => new { Term = g.Key, MistakeCount = g.Count() })
            .OrderByDescending(x => x.MistakeCount)
            .Take(5)
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
}
