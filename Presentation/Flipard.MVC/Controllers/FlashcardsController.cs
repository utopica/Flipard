using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

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
}