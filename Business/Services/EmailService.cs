using Data.Interfaces;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace Business.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["Email:Smtp:Host"],
            int.Parse(_config["Email:Smtp:Port"]),
            SecureSocketOptions.None
        );

        if (!string.IsNullOrEmpty(_config["Email:Smtp:User]"]))
        {
            await smtp.AuthenticateAsync(
                _config["Email:Smtp:User"],
                _config["Email:Smtp:Pass"]
            );
        }

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
