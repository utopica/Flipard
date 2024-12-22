namespace Flipard.Application.Services;

public interface IOcrService
{
    string ExtractTextFromFile(string filePath);
}