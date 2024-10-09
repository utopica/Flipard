using Flipard.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Flipard.Persistence.Concretes;

public class EmailService : IEmailService
{
    private readonly string _apiKey;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"];;
    }

    public async Task SendEmailAsync(string toEmail, string fromEmail, string body)
    {
        var apiKey = _apiKey;
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "Me");
        var subject = $"Movie Recommendation : {body}";
        var to = new EmailAddress(toEmail, "Dear");
        var plainTextContent = "You have been recommended a movie. Please check it out.";
        var htmlContent = "";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);

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