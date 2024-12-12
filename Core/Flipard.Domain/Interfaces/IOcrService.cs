namespace Flipard.Domain.Interfaces;

public interface IOcrService
{
    string ExtractTextFromFile(string filePath);
}