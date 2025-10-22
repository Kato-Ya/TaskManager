namespace NotificationService.Interfaces;
public interface IEmailSenderService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
