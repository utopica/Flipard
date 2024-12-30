using System.Security.Claims;
using Flipard.Domain.Identity;
using Flipard.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flipard.MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizAttemptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public QuizAttemptsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("user-attempts")]
    public async Task<IActionResult> GetUserAttempts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        var attempts = await _context.QuizAttempts
            .Where(a => a.UserId == Guid.Parse(userId))
            .Select(a => new
            {
                a.AttemptDate,
                a.Accuracy,
                a.TotalQuestions,
                a.CorrectAnswers
            })
            .ToListAsync();

        return Ok(attempts);
    }
}