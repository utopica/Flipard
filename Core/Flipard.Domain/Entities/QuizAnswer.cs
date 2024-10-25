using System.ComponentModel.DataAnnotations.Schema;
using Flipard.Domain.Common;

namespace Flipard.Domain.Entities;

public class QuizAnswer : EntityBase<Guid>
{
    public Guid QuizAttemptId { get; set; }
    public Guid VocabularyId { get; set; }
    public string UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    
    [ForeignKey("QuizAttemptId")]
    public QuizAttempt QuizAttempt { get; set; }
    [ForeignKey("VocabularyId")]
    public Vocabulary Vocabulary { get; set; }
}