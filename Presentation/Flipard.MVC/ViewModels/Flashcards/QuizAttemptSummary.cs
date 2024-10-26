namespace Flipard.MVC.Models.Flashcards;

public class QuizAttemptSummary
{
    public DateTime AttemptDate { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public double Accuracy { get; set; }
    public int TimeTakenSeconds { get; set; }
}