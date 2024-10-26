namespace Flipard.MVC.Models.Flashcards;

public class QuizResultViewModel
{
    public Guid DeckId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double TimeTaken { get; set; } 
    public List<QuizAnswerDetail> AnswerDetails { get; set; }
}

public class QuizAnswerDetail
{
    public Guid VocabularyId { get; set; }
    public string UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
}