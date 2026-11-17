using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Models;

namespace CEMS.Services;

/// <summary>
/// Business rules for event registrations: seat limits, duplicate prevention,
/// cancellation, updates, attendance - and the audit log entry for every change.
/// </summary>
public class RegistrationService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notify;

    public RegistrationService(AppDbContext db, NotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public (bool Ok, string Message) Register(int userId, int eventId, string? remarks)
    {
        var ev = _db.Events.Include(e => e.Registrations).FirstOrDefault(e => e.EventId == eventId);
        if (ev == null) return (false, "Event not found.");
        if (ev.Status != "Active") return (false, "This event is not open for registration.");
        if (ev.EventDate <= DateTime.Now) return (false, "This event has already started or finished.");
        if (ev.Registrations.Any(r => r.UserId == userId && r.Status == "Registered"))
            return (false, "You are already registered for this event.");
        if (ev.SeatsTaken >= ev.MaxSeats) return (false, "Sorry, this event is full.");

        var reg = new Registration { UserId = userId, EventId = eventId, Remarks = Clean(remarks, 200) };
        _db.Registrations.Add(reg);
        _db.RegistrationLogs.Add(new RegistrationLog
        {
            Registration = reg,
            ActorUserId = userId,
            Action = "Registered",
            Details = Clean("Registered for " + ev.Title, 500)
        });
        _db.SaveChanges();

        _notify.Notify(userId,
            $"Registration confirmed: {ev.Title} on {ev.EventDate:dd MMM yyyy, hh:mm tt} at {ev.Venue}.",
            "Registration confirmation");
        return (true, "Registration successful! A confirmation was added to your notifications.");
    }

    public (bool Ok, string Message) Cancel(int registrationId, int userId, string? reason)
    {
        var reg = _db.Registrations.Include(r => r.Event)
            .FirstOrDefault(r => r.RegistrationId == registrationId && r.UserId == userId);
        if (reg == null) return (false, "Registration not found.");
        if (reg.Status != "Registered") return (false, "Only active registrations can be cancelled.");
        if (reg.Event.EventDate <= DateTime.Now) return (false, "This event has already started, so the registration can no longer be cancelled.");

        reg.Status = "Cancelled";
        reg.CancelledAt = DateTime.Now;
        reg.CancelReason = Clean(reason, 400) ?? "Cancelled by participant";
        _db.RegistrationLogs.Add(new RegistrationLog
        {
            RegistrationId = reg.RegistrationId,
            ActorUserId = userId,
            Action = "Cancelled",
            Details = Clean("Reason: " + reg.CancelReason, 500)
        });
        _db.SaveChanges();

        _notify.Notify(userId, $"Your registration for {reg.Event.Title} has been cancelled.", "Registration cancelled");
        return (true, "Registration cancelled. Your seat has been released.");
    }

    public (bool Ok, string Message) UpdateRemarks(int registrationId, int userId, string? remarks)
    {
        var reg = _db.Registrations.Include(r => r.Event)
            .FirstOrDefault(r => r.RegistrationId == registrationId && r.UserId == userId);
        if (reg == null) return (false, "Registration not found.");
        if (reg.Status != "Registered") return (false, "Only active registrations can be updated.");
        if (reg.Event.EventDate <= DateTime.Now) return (false, "This event has already started, so the registration can no longer be updated.");

        var oldValue = reg.Remarks;
        var newValue = Clean(remarks, 200);
        if (oldValue == newValue) return (false, "No changes were made.");

        reg.Remarks = newValue;
        _db.RegistrationLogs.Add(new RegistrationLog
        {
            RegistrationId = reg.RegistrationId,
            ActorUserId = userId,
            Action = "Updated",
            Details = Clean($"Remarks changed from '{oldValue ?? "(none)"}' to '{newValue ?? "(none)"}'", 500)
        });
        _db.SaveChanges();

        _notify.Notify(userId, $"Your registration details for {reg.Event.Title} were updated.", "Registration updated");
        return (true, "Registration updated.");
    }

    /// <summary>Cancels every active registration of an event that the organiser cancelled.</summary>
    public int CancelAllForEvent(Event ev, int actorUserId, string reason)
    {
        var regs = _db.Registrations.Where(r => r.EventId == ev.EventId && r.Status == "Registered").ToList();
        foreach (var r in regs)
        {
            r.Status = "Cancelled";
            r.CancelledAt = DateTime.Now;
            r.CancelReason = Clean(reason, 500);
            _db.RegistrationLogs.Add(new RegistrationLog
            {
                RegistrationId = r.RegistrationId,
                ActorUserId = actorUserId,
                Action = "Cancelled",
                Details = Clean(reason, 500)
            });
        }
        _db.SaveChanges();

        foreach (var r in regs)
            _notify.Notify(r.UserId,
                $"The event \"{ev.Title}\" has been cancelled by the organiser. Your registration was cancelled automatically.",
                "Event cancelled");
        return regs.Count;
    }

    public (bool Ok, string Message) SetAttendance(int registrationId, int actorUserId, bool attended)
    {
        var reg = _db.Registrations.Include(r => r.Event).FirstOrDefault(r => r.RegistrationId == registrationId);
        if (reg == null || reg.Status == "Cancelled") return (false, "Registration not found or already cancelled.");

        var newStatus = attended ? "Attended" : "Registered";
        if (reg.Status == newStatus) return (false, "No change.");

        reg.Status = newStatus;
        _db.RegistrationLogs.Add(new RegistrationLog
        {
            RegistrationId = reg.RegistrationId,
            ActorUserId = actorUserId,
            Action = attended ? "Attended" : "Attendance reverted",
            Details = attended ? "Marked as attended by organiser" : "Attendance mark removed by organiser"
        });
        _db.SaveChanges();

        if (attended)
            _notify.Notify(reg.UserId,
                $"Thank you for attending {reg.Event.Title}! Please share your feedback on the event page.",
                "Share your feedback");
        return (true, attended ? "Marked as attended." : "Attendance mark removed.");
    }

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
