using System.ComponentModel.DataAnnotations.Schema;
using Flipard.Domain.Common;
using Flipard.Domain.Identity;

namespace Flipard.Domain.Entities;

public class QuizAttempt : EntityBase<Guid>
{
    public Guid DeckId { get; set; }
    public Guid UserId { get; set; } 
    public DateTime AttemptDate { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double Accuracy { get; set; }
    public int TimeTakenSeconds { get; set; }
    
    // Navigation properties
    [ForeignKey("DeckId")]
    public Deck Deck { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    public ICollection<QuizAnswer> Answers { get; set; }
}