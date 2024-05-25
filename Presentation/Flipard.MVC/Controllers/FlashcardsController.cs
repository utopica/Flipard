using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class FlashcardsController : Controller
{
    private readonly ApplicationDbContext _appContext;

    public FlashcardsController(ApplicationDbContext appContext)
    {
        _appContext = appContext;
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
            IsReadOnly = isReadOnly // Set IsReadOnly property
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

    [HttpPost]
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

    [HttpPost]
    public IActionResult UpdateCard([FromBody] TermMeaningViewModel updatedCard)
    {
        if (updatedCard == null || updatedCard.Id == Guid.Empty)
        {
            return BadRequest("Invalid card data.");
        }

        var card = _appContext.Cards.Include(c => c.Vocabulary).FirstOrDefault(c => c.Vocabulary.Id == updatedCard.Id);

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
}
