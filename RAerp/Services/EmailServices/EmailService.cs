using Microsoft.Extensions.Options;
using RAerp.Models.EmailModel;
using System.Net;
using System.Net.Mail;

namespace RAerp.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                UseDefaultCredentials = false,
                EnableSsl = _smtp.EnableSsl,
                Credentials = new NetworkCredential(
                    _smtp.Username,
                    _smtp.Password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(
                    _smtp.SenderEmail,
                    _smtp.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
