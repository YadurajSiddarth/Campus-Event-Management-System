using System.Net;
using System.Net.Mail;

namespace CEMS.Services;

public interface IEmailService
{
    void Send(string toEmail, string subject, string body);
}

/// <summary>
/// Sends e-mail through SMTP when "Email:Enabled" is true in appsettings.json.
/// When disabled (default) the message is only written to the application log, so the
/// system works without any mail server. A mail failure never breaks the user's action.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _log;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> log)
    {
        _config = config;
        _log = log;
    }

    public void Send(string toEmail, string subject, string body)
    {
        var cfg = _config.GetSection("Email");
        if (!cfg.GetValue<bool>("Enabled"))
        {
            _log.LogInformation("[Email disabled] To: {To} | Subject: {Subject} | {Body}", toEmail, subject, body);
            return;
        }

        try
        {
            using var client = new SmtpClient(cfg["Host"], cfg.GetValue<int>("Port"))
            {
                EnableSsl = cfg.GetValue("EnableSsl", true),
                Credentials = new NetworkCredential(cfg["Username"], cfg["Password"])
            };
            var from = cfg["From"] ?? cfg["Username"] ?? "noreply@cems.edu.my";
            using var message = new MailMessage(from, toEmail, "[CEMS] " + subject, body);
            client.Send(message);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Could not send e-mail to {To}", toEmail);
        }
    }
}
