using System.Net;
using System.Net.Mail;

namespace RecamNewBackend.Common.Utilities;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _configuration["Email:Host"];
        var port = int.Parse(_configuration["Email:Port"]!);
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];

        var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(username!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }
}