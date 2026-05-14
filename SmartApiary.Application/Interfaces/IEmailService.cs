namespace SmartApiary.Application.Interfaces;

public interface IEmailService
{
    Task SendActivationEmailAsync(string toEmail, string toName, string activationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
    Task SendAlertEmailAsync(string toEmail, string toName, string subject, string body);
}
