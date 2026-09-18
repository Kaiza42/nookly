using System.Net;
using System.Net.Mail;

namespace Nookly.Api.Email;

public sealed class EmailSender(IConfiguration configuration, IWebHostEnvironment environment) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken token)
    {
        var host = configuration["NOOKLY_SMTP_HOST"];
        if (string.IsNullOrWhiteSpace(host))
        {
            var directory = Path.Combine(environment.ContentRootPath, ".email-outbox");
            Directory.CreateDirectory(directory);
            var file = Path.Combine(directory, $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.txt");
            await File.WriteAllTextAsync(file, $"To: {recipient}\nSubject: {subject}\n\n{body}", token);
            return;
        }

        using var message = new MailMessage(configuration["NOOKLY_SMTP_FROM"] ?? "noreply@nookly.local", recipient, subject, body);
        using var client = new SmtpClient(host, int.TryParse(configuration["NOOKLY_SMTP_PORT"], out var port) ? port : 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(configuration["NOOKLY_SMTP_USER"], configuration["NOOKLY_SMTP_PASSWORD"])
        };
        await client.SendMailAsync(message, token);
    }
}
