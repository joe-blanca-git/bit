using apiBit.Interfaces;
using System.Net;
using System.Net.Mail;

namespace apiBit.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var smtpClient = new SmtpClient(settings["SmtpServer"])
            {
                Port = int.Parse(settings["Port"]),
                Credentials = new NetworkCredential(settings["SenderEmail"], settings["Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(settings["SenderEmail"], settings["SenderName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}