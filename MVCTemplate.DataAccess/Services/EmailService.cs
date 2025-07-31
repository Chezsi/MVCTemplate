using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MVCTemplate.DataAccess.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithImageAsync(string to, string subject, string htmlBody, byte[] imageBytes, string fileName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["Email:Smtp:Host"])
            {
                Port = int.Parse(_config["Email:Smtp:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Smtp:Username"],
                    _config["Email:Smtp:Password"]
                ),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Smtp:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            await smtpClient.SendMailAsync(mail);
        }

        public async Task SendEmailWithImageAsync(string to, string subject, string htmlBody, byte[] imageBytes, string fileName)
        {
            var smtpClient = new SmtpClient(_config["Email:Smtp:Host"])
            {
                Port = int.Parse(_config["Email:Smtp:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Smtp:Username"],
                    _config["Email:Smtp:Password"]
                ),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Smtp:From"]),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            if (imageBytes != null && imageBytes.Length > 0)
            {
                var imageStream = new MemoryStream(imageBytes);
                var attachment = new Attachment(imageStream, fileName, "image/png");
                mail.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mail);
        }
    }
}
