using Flipard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Flipard.MVC.Controllers
{
    public class OcrController : Controller
    {
        private readonly IOcrService _ocrService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<OcrController> _logger;

        public OcrController(
            IOcrService ocrService, 
            IWebHostEnvironment environment,
            ILogger<OcrController> logger)
        {
            _ocrService = ocrService ?? 
                throw new ArgumentNullException(nameof(ocrService));
            _environment = environment ?? 
                throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult UploadFile()
        {
            return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please upload a valid file.");
                _logger.LogWarning("Attempted to upload an empty or null file");
                return View();
            }

            string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".bmp", ".tiff" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("file", "Unsupported file type. Please upload a PDF or image.");
                _logger.LogWarning("Attempted to upload an unsupported file type: {FileExtension}", fileExtension);
                return View();
            }

            string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadFolder);

            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving uploaded file");
                ModelState.AddModelError("file", "An error occurred while saving the file.");
                return View();
            }

            try
            {
                string extractedText = _ocrService.ExtractTextFromFile(filePath);
                
                System.IO.File.Delete(filePath);

                ViewBag.ExtractedText = extractedText;
                return View("OcrResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from file");
                ModelState.AddModelError("", $"An error occurred during text extraction: {ex.Message}");
                return View();
            }
        }
    }
}