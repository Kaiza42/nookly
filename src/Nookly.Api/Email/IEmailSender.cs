namespace Nookly.Api.Email;

public interface IEmailSender { Task SendAsync(string recipient, string subject, string body, CancellationToken token); }
