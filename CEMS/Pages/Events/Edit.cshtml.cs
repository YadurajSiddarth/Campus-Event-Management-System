using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Events;

[SessionAuthorize(Roles = "Staff,Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly EventResourceService _resources;
    private readonly RegistrationService _registrations;
    private readonly NotificationService _notify;

    public EditModel(AppDbContext db, EventResourceService resources, RegistrationService registrations, NotificationService notify)
    {
        _db = db;
        _resources = resources;
        _registrations = registrations;
        _notify = notify;
    }

    public Event Event { get; set; } = null!;

    [BindProperty]
    public EventInput Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> Files { get; set; } = new();

    private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;

    private bool CanManage(Event ev) =>
        HttpContext.Session.GetString("UserRole") == "Admin" || ev.OrganiserId == UserId;

    private Event? Load(int id) =>
        _db.Events.Include(e => e.Registrations).Include(e => e.Resources).FirstOrDefault(e => e.EventId == id);

    public IActionResult OnGet(int id)
    {
        var ev = Load(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");
        Event = ev;

        Input = new EventInput
        {
            Title = ev.Title,
            Description = ev.Description,
            EventDate = ev.EventDate.AddTicks(-(ev.EventDate.Ticks % TimeSpan.TicksPerMinute)),
            Venue = ev.Venue,
            Category = ev.Category,
            MaxSeats = ev.MaxSeats
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var ev = Load(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");
        Event = ev;

        if (ev.Status != "Active")
            ModelState.AddModelError("", "Only active events can be edited.");

        var dateChanged = Math.Abs((Input.EventDate - ev.EventDate).TotalMinutes) >= 1;
        if (dateChanged && Input.EventDate <= DateTime.Now)
            ModelState.AddModelError("Input.EventDate", "The new event date must be in the future.");

        if (Input.MaxSeats < ev.SeatsTaken)
            ModelState.AddModelError("Input.MaxSeats", $"{ev.SeatsTaken} seats are already taken, so the limit cannot be lower than that.");

        var fileError = _resources.Validate(Files);
        if (fileError != null)
            ModelState.AddModelError("Files", fileError);

        if (!ModelState.IsValid) return Page();

        var venue = Input.Venue.Trim();
        var title = Input.Title.Trim();
        var importantChange = dateChanged || venue != ev.Venue || title != ev.Title;

        ev.Title = title;
        ev.Description = Input.Description.Trim();
        ev.EventDate = dateChanged ? Input.EventDate : ev.EventDate;
        ev.Venue = venue;
        ev.Category = Input.Category;
        ev.MaxSeats = Input.MaxSeats;
        _db.SaveChanges();

        await _resources.SaveAsync(ev.EventId, Files);

        if (importantChange)
        {
            var userIds = ev.Registrations.Where(r => r.Status == "Registered").Select(r => r.UserId).Distinct().ToList();
            foreach (var uid in userIds)
                _notify.Notify(uid,
                    $"Event updated: \"{ev.Title}\" is now on {ev.EventDate:dd MMM yyyy, hh:mm tt} at {ev.Venue}.",
                    "Event updated");
        }

        TempData["Success"] = "Event updated successfully.";
        return RedirectToPage("Detail", new { id = ev.EventId });
    }

    public IActionResult OnPostCancelEvent(int id, string? reason)
    {
        var ev = Load(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");

        if (ev.Status != "Active")
        {
            TempData["Error"] = "Only active events can be cancelled.";
            return RedirectToPage(new { id });
        }

        ev.Status = "Cancelled";
        _db.SaveChanges();

        var text = string.IsNullOrWhiteSpace(reason) ? "Event cancelled by organiser" : "Event cancelled by organiser: " + reason.Trim();
        if (text.Length > 450) text = text[..450];
        var count = _registrations.CancelAllForEvent(ev, UserId, text);

        TempData["Success"] = $"Event cancelled. {count} registered participant(s) were notified.";
        return RedirectToPage("Detail", new { id });
    }

    public IActionResult OnPostDeleteResource(int id, int resourceId)
    {
        var ev = Load(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");

        _resources.Delete(resourceId, id);
        TempData["Success"] = "Resource removed.";
        return RedirectToPage(new { id });
    }
}
