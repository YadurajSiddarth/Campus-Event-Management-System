using CEMS.Data;
using CEMS.Models;

namespace CEMS.Services;

/// <summary>Creates an in-app notification and also e-mails it to the user.</summary>
public class NotificationService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public NotificationService(AppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public void Notify(int userId, string message, string subject = "Notification")
    {
        if (message.Length > 500) message = message[..500];
        _db.Notifications.Add(new Notification { UserId = userId, Message = message });
        _db.SaveChanges();

        var email = _db.Users.Where(u => u.UserId == userId).Select(u => u.Email).FirstOrDefault();
        if (!string.IsNullOrEmpty(email))
            _email.Send(email, subject, message);
    }
}
