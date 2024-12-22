using Flipard.Domain.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using System.Text;
using Flipard.Application.Services;
using Tesseract;

namespace Flipard.Infrastructure.Services;

public class TesseractOcrService : IOcrService
{
    private readonly string _tessDataPath;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(
        string tessDataPath, 
        ILogger<TesseractOcrService> logger)
    {
        _tessDataPath = tessDataPath ?? 
            throw new ArgumentNullException(nameof(tessDataPath));
        _logger = logger ?? 
            throw new ArgumentNullException(nameof(logger));
    }

    public string ExtractTextFromFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogError("File path is null or empty");
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException("File not found.", filePath);
        }

        try
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            return fileExtension switch
            {
                ".pdf" => ExtractTextFromPdf(filePath),
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tiff" => ExtractTextFromImage(filePath),
                _ => throw new NotSupportedException($"File type {fileExtension} is not supported.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from file: {FilePath}", filePath);
            throw;
        }
    }

    private string ExtractTextFromImage(string imagePath)
    {
        try
        {
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            
            string extractedText = page.GetText();
            _logger.LogInformation("Successfully extracted text from image: {ImagePath}", imagePath);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image: {ImagePath}", imagePath);
            throw;
        }
    }

    private string ExtractTextFromPdf(string pdfPath)
    {
        try
        {
            using var reader = new PdfReader(pdfPath);
            using var document = new PdfDocument(reader);

            var result = new StringBuilder();
            for (int i = 1; i <= document.GetNumberOfPages(); i++)
            {
                var strategy = new LocationTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(document.GetPage(i), strategy);
                result.AppendLine(pageText);
            }

            _logger.LogInformation("Successfully extracted text from PDF: {PdfPath}", pdfPath);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF: {PdfPath}", pdfPath);
            throw;
        }
    }
}