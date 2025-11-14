using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using AuthenticationService.Models;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendDelegationEmailAsync(Delegation delegation, User delegator, User delegatee)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var smtpUsername = emailSettings["SmtpUsername"];
            var smtpPassword = emailSettings["SmtpPassword"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"] ?? "Authentication Service";

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Email settings not configured");
                return;
            }

            if (string.IsNullOrEmpty(delegatee.Email))
            {
                _logger.LogWarning("Delegatee email not found");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(delegatee.FullName ?? delegatee.Username, delegatee.Email));
            message.Subject = $"Thông báo ủy quyền từ {delegator.FullName ?? delegator.Username}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Thông báo ủy quyền</h2>
                    <p>Xin chào <strong>{delegatee.FullName ?? delegatee.Username}</strong>,</p>
                    <p>Bạn đã được <strong>{delegator.FullName ?? delegator.Username}</strong> ủy quyền trong khoảng thời gian:</p>
                    <ul>
                        <li><strong>Từ ngày:</strong> {delegation.StartDate:dd/MM/yyyy}</li>
                        <li><strong>Đến ngày:</strong> {delegation.EndDate:dd/MM/yyyy}</li>
                    </ul>
                    {(string.IsNullOrEmpty(delegation.Reason) ? "" : $"<p><strong>Lý do:</strong> {delegation.Reason}</p>")}
                    <p>Trong thời gian này, khi đăng nhập, bạn sẽ có quyền truy cập với vai trò của người ủy quyền.</p>
                    <p>Trân trọng,<br/>Authentication Service</p>
                "
            };

            // Attach PDF if available
            if (!string.IsNullOrEmpty(delegation.PdfPath) && File.Exists(delegation.PdfPath))
            {
                bodyBuilder.Attachments.Add(delegation.PdfPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUsername, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Delegation email sent successfully to {Email}", delegatee.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending delegation email");
            throw;
        }
    }
}

