using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Events;

public class DetailModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly RegistrationService _registrations;
    private readonly NotificationService _notify;

    public DetailModel(AppDbContext db, RegistrationService registrations, NotificationService notify)
    {
        _db = db;
        _registrations = registrations;
        _notify = notify;
    }

    public Event? Event { get; set; }
    public int SeatsLeft { get; set; }
    public Registration? MyRegistration { get; set; }
    public bool IsLoggedIn { get; set; }
    public bool HasAttended { get; set; }
    public bool HasGivenFeedback { get; set; }
    public bool CanManage { get; set; }
    public List<Feedback> Feedbacks { get; set; } = new();
    public List<Enquiry> MyEnquiries { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        Event = _db.Events.Include(e => e.Organiser).Include(e => e.Registrations).Include(e => e.Resources)
            .FirstOrDefault(e => e.EventId == id);
        if (Event == null) return Page();

        SeatsLeft = Event.MaxSeats - Event.SeatsTaken;
        Feedbacks = _db.Feedbacks.Include(f => f.User).Where(f => f.EventId == id)
            .OrderByDescending(f => f.SubmittedAt).ToList();

        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        IsLoggedIn = userId != null;
        if (userId != null)
        {
            MyRegistration = Event.Registrations.FirstOrDefault(r => r.UserId == userId && r.Status == "Registered");
            HasAttended = Event.Registrations.Any(r => r.UserId == userId && r.Status == "Attended");
            HasGivenFeedback = Feedbacks.Any(f => f.UserId == userId);
            CanManage = role == "Admin" || (role == "Staff" && Event.OrganiserId == userId);
            MyEnquiries = _db.Enquiries.Where(q => q.EventId == id && q.UserId == userId)
                .OrderByDescending(q => q.SentAt).ToList();
        }
        return Page();
    }

    public IActionResult OnPostRegister(int eventId, string? remarks)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        var result = _registrations.Register(userId.Value, eventId, remarks);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return RedirectToPage(new { id = eventId });
    }

    public IActionResult OnPostUpdate(int eventId, int registrationId, string? remarks)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        var result = _registrations.UpdateRemarks(registrationId, userId.Value, remarks);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return RedirectToPage(new { id = eventId });
    }

    public IActionResult OnPostCancel(int eventId, int registrationId, string? reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        var result = _registrations.Cancel(registrationId, userId.Value, reason);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return RedirectToPage(new { id = eventId });
    }

    public IActionResult OnPostFeedback(int eventId, int rating, string? comments)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        if (!_db.Registrations.Any(r => r.EventId == eventId && r.UserId == userId && r.Status == "Attended"))
            TempData["Error"] = "Only participants who attended the event can submit feedback.";
        else if (rating < 1 || rating > 5)
            TempData["Error"] = "Please choose a rating between 1 and 5.";
        else if (comments != null && comments.Length > 1000)
            TempData["Error"] = "Comments are limited to 1000 characters.";
        else if (_db.Feedbacks.Any(f => f.EventId == eventId && f.UserId == userId))
            TempData["Error"] = "You have already submitted feedback for this event.";
        else
        {
            _db.Feedbacks.Add(new Feedback
            {
                EventId = eventId,
                UserId = userId.Value,
                Rating = rating,
                Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim()
            });
            _db.SaveChanges();
            TempData["Success"] = "Feedback submitted. Thank you!";
        }
        return RedirectToPage(new { id = eventId });
    }

    public IActionResult OnPostEnquiry(int eventId, string subject, string message)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Account/Login");

        subject = (subject ?? "").Trim();
        message = (message ?? "").Trim();
        var ev = _db.Events.Find(eventId);

        if (ev == null) TempData["Error"] = "Event not found.";
        else if (subject.Length == 0 || subject.Length > 200) TempData["Error"] = "Please enter a subject (up to 200 characters).";
        else if (message.Length == 0 || message.Length > 1000) TempData["Error"] = "Please enter a message (up to 1000 characters).";
        else
        {
            _db.Enquiries.Add(new Enquiry { EventId = eventId, UserId = userId.Value, Subject = subject, Message = message });
            _db.SaveChanges();

            var senderName = HttpContext.Session.GetString("UserName") ?? "A participant";
            _notify.Notify(ev.OrganiserId, $"New enquiry on \"{ev.Title}\" from {senderName}: {subject}", "New event enquiry");
            TempData["Success"] = "Enquiry sent to the organiser.";
        }
        return RedirectToPage(new { id = eventId });
    }
}
