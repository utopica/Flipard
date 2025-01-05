namespace Flipard.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string fromEmail, string body);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
    Task SendEmailVerificationAsync(string to, string token);
}