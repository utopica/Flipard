namespace Flipard.MVC.Models.Flashcards;

public class QuizStatisticsViewModel
{
    public string DeckName { get; set; }
    public int TotalAttempts { get; set; }
    public double AverageAccuracy { get; set; }
    public double BestAccuracy { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public Dictionary<string, int> MostMistakenTerms { get; set; }
    public List<QuizAttemptSummary> RecentAttempts { get; set; }
}