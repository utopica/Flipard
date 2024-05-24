using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;

public class FlashcardsController : Controller
{
    private readonly ApplicationDbContext _Appcontext;

    public FlashcardsController(ApplicationDbContext appContext)
    {
        _Appcontext = appContext;
    }

    public IActionResult Index(Guid id)
    {
        var deck = _Appcontext.Decks
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

    [HttpDelete]
    public IActionResult DeleteCard(Guid id)
    {
        var card = _Appcontext.Cards.FirstOrDefault(c => c.Vocabulary.Id == id);

        if (card == null)
        {
            return NotFound();
        }

        _Appcontext.Cards.Remove(card);
        _Appcontext.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public IActionResult DeleteDeck(Guid id)
    {
        var deck = _Appcontext.Decks.FirstOrDefault(d => d.Id == id);

        if (deck == null)
        {
            return NotFound();
        }

        _Appcontext.Decks.Remove(deck);
        _Appcontext.SaveChanges();

        return RedirectToAction(nameof(Index), "Home");
    }

    [HttpPost]
    public IActionResult UpdateCard([FromBody] TermMeaningViewModel updatedCard)
    {
        if (updatedCard == null || updatedCard.Id == Guid.Empty)
        {
            return BadRequest("Invalid card data.");
        }

        var card = _Appcontext.Cards.Include(c => c.Vocabulary).FirstOrDefault(c => c.Vocabulary.Id == updatedCard.Id);

        if (card == null)
        {
            return NotFound();
        }

        card.Vocabulary.Term = updatedCard.Term;
        card.Vocabulary.Meaning = updatedCard.Meaning;

        _Appcontext.SaveChanges();

        return Ok();
    }

}