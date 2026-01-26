namespace WebsiteE_Commerce.Services.Email
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
