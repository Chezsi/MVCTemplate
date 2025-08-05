using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;

namespace MVCTemplate.DataAccess.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithImageAsync(string to, string subject, string htmlBody, byte[] imageBytes, string fileName);
        Task SendEmailWithImageAndEmbeddedIconsAsync(string to, string subject, string htmlBody, byte[] imageBytes, string fileName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = CreateSmtpClient();

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
            var smtpClient = CreateSmtpClient();

            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Smtp:From"]),
                Subject = subject,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            // Plain HTML body without CID support
            mail.Body = htmlBody;

            if (imageBytes != null && imageBytes.Length > 0)
            {
                var imageStream = new MemoryStream(imageBytes);
                var attachment = new Attachment(imageStream, fileName, "image/png");
                mail.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mail);
        }

        public async Task SendEmailWithImageAndEmbeddedIconsAsync(string to, string subject, string htmlBody, byte[] imageBytes, string fileName)
        {
            var smtpClient = CreateSmtpClient();

            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Smtp:From"]),
                Subject = subject,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            // Setup HTML view for CID image embedding
            var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            // Embed icons from wwwroot/LogosIcons
            EmbedIcon(htmlView, "fb-icon", "LogosIcons/fb.png");
            EmbedIcon(htmlView, "ig-icon", "LogosIcons/ig.png");
            EmbedIcon(htmlView, "twitter-icon", "LogosIcons/twitter.png");

            mail.AlternateViews.Add(htmlView);

            // Attach product image
            if (imageBytes != null && imageBytes.Length > 0)
            {
                var imageStream = new MemoryStream(imageBytes);
                var attachment = new Attachment(imageStream, fileName, "image/png");
                mail.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mail);
        }

        private void EmbedIcon(AlternateView view, string contentId, string relativePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                var resource = new LinkedResource(fullPath, MediaTypeNames.Image.Png)
                {
                    ContentId = contentId,
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = { MediaType = "image/png" }
                };

                view.LinkedResources.Add(resource);
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_config["Email:Smtp:Host"])
            {
                Port = int.Parse(_config["Email:Smtp:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Smtp:Username"],
                    _config["Email:Smtp:Password"]
                ),
                EnableSsl = true
            };
        }
    }
}
