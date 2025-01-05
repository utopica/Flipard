using Flipard.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Flipard.Persistence.Concretes;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly ILogger<EmailService> _logger; 

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _apiKey = configuration["SendGrid:ApiKey"];;
    }

    public async Task SendEmailAsync(string toEmail, string fromEmail, string body)
    {
        try{
        var apiKey = _apiKey;
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "Flipard");
        var subject = $"Şifre Sıfırlama Maili ";
        var to = new EmailAddress(toEmail, "Dear");
        var plainTextContent = "Gönderilen şifre sıfırlama bağlantısına tıklayıp şifrenizi yenileyiniz.";
        var htmlContent = $"{body}";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogError($"Failed to send email. Status Code: {response.StatusCode}, Body: {responseBody}");
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
    }catch (Exception ex)
        {
            _logger.LogError($"Error sending email: {ex.Message}");
            throw;
        }
    }
    
    public Task SendPasswordResetEmailAsync(string to, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendEmailVerificationAsync(string to, string token)
    {
        throw new NotImplementedException();
    }
}