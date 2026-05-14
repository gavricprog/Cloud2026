using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SmartApiary.Application.Interfaces;

namespace SmartApiary.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendActivationEmailAsync(string toEmail, string toName, string activationLink)
    {
        var subject = "Aktivacija naloga - Smart Apiary";
        var body = $"""
            <h2>Dobrodošli na Smart Apiary platformu, {toName}!</h2>
            <p>Administrator vam je kreirao nalog. Kliknite na link ispod da biste postavili svoju lozinku i aktivirali nalog:</p>
            <p><a href="{activationLink}">Aktiviraj nalog</a></p>
            <p>Link važi 24 sata.</p>
            """;
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        var subject = "Resetovanje lozinke - Smart Apiary";
        var body = $"""
            <h2>Zdravo, {toName}!</h2>
            <p>Primili smo zahtev za resetovanje vaše lozinke. Kliknite na link ispod:</p>
            <p><a href="{resetLink}">Resetuj lozinku</a></p>
            <p>Link važi 1 sat. Ukoliko niste zatražili resetovanje, ignorišite ovu poruku.</p>
            """;
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendAlertEmailAsync(string toEmail, string toName, string subject, string body)
    {
        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_config["Email:SenderEmail"], _config["Email:Password"])
        };

        var message = new MailMessage
        {
            From = new MailAddress(_config["Email:SenderEmail"]!, _config["Email:SenderName"]),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}
