using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ElasoftCommunityManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
        Task Send2FASetupEmailAsync(string to, string qrCodeUrl);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _senderEmail = _configuration["Email:SenderEmail"];
            _senderName = _configuration["Email:SenderName"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_senderName, _senderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password. Your temporary password is:</p>
                <h3>{resetToken}</h3>
                <p>Please use this password to log in and change your password immediately.</p>
                <p>If you did not request this password reset, please ignore this email.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task Send2FASetupEmailAsync(string to, string qrCodeUrl)
        {
            var subject = "Two-Factor Authentication Setup";
            var body = $@"
                <h2>Two-Factor Authentication Setup</h2>
                <p>You have enabled two-factor authentication. Please scan the QR code below with your authenticator app:</p>
                <img src=""{qrCodeUrl}"" alt=""QR Code"" />
                <p>If you cannot scan the QR code, please contact support.</p>";

            await SendEmailAsync(to, subject, body);
        }
    }
}