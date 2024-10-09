namespace Flipard.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
    Task SendEmailVerificationAsync(string to, string token);
}