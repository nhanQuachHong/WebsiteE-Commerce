namespace WebsiteE_Commerce.Services.Email
{
    public sealed class ConsoleEmailSender : IAppEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            Console.WriteLine("=== EMAIL (DEV) ===");
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine("Body:");
            Console.WriteLine(htmlMessage);
            Console.WriteLine("===================");

            return Task.CompletedTask;
        }
    }
}
