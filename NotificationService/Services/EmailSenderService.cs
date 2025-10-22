using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using NotificationService.Interfaces;

namespace NotificationService.Services;
public class EmailSenderService : IEmailSenderService, IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSenderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpHost = _config["Email: SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"]!);
        var smtpUser = _config["Email:User"];
        var smtpPass = _config["Email:Password"];
        var fromUser = _config["Email:From"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            //EnableSsl = true
            EnableSsl = false
        };

        var mail = new MailMessage(fromUser!, email, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mail);
    }
}
