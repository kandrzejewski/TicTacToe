using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TicTacToe.Options;

namespace TicTacToe.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailServiceOptions _emailServiceOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailServiceOptions> emailServiceOptions, ILogger<EmailService> logger)
        {
            _emailServiceOptions = emailServiceOptions.Value;
            _logger = logger;
        }

        public Task SendEmail(string emailTo, string subject, string message)
        {
            try
            {
                _logger.LogInformation($"##Start metody sendEmail## Rozpoczęcie wysyłania wiadomości do {emailTo}");

                using (SmtpClient client = new SmtpClient(_emailServiceOptions.MailServer,
                int.Parse(_emailServiceOptions.MailPort)))
                {
                    if (bool.Parse(_emailServiceOptions.UseSSL) == true)
                    {
                        client.EnableSsl = true;
                    }

                    if (!string.IsNullOrEmpty(_emailServiceOptions.UserId))
                    {
                        client.Credentials = new NetworkCredential(_emailServiceOptions.UserId,
                            _emailServiceOptions.Password);
                    }

                    client.Send(new MailMessage("example@example.com", emailTo, subject, message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można wysłać wiadomości e-mail {ex}");
            }
            return Task.CompletedTask;
        }
    }
}
