using Microsoft.AspNetCore.Identity.UI.Services;

namespace ArtSharing.Web.Service
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("📧 [FAKE EMAIL]");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");

            return Task.CompletedTask;
        }  
    }
}
