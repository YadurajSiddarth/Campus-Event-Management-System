using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Events;

[SessionAuthorize(Roles = "Staff,Admin")]
public class AttendanceModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly RegistrationService _registrations;

    public AttendanceModel(AppDbContext db, RegistrationService registrations)
    {
        _db = db;
        _registrations = registrations;
    }

    public Event Event { get; set; } = null!;
    public List<Registration> Participants { get; set; } = new();

    private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;

    private bool CanManage(Event ev) =>
        HttpContext.Session.GetString("UserRole") == "Admin" || ev.OrganiserId == UserId;

    private IActionResult? Load(int id)
    {
        var ev = _db.Events.Find(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");
        Event = ev;
        Participants = _db.Registrations.Include(r => r.User)
            .Where(r => r.EventId == id && r.Status != "Cancelled")
            .OrderBy(r => r.User.FullName).ToList();
        return null;
    }

    public IActionResult OnGet(int id) => Load(id) ?? Page();

    public IActionResult OnPostMark(int id, int registrationId, bool attended)
    {
        var ev = _db.Events.Find(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");

        var belongs = _db.Registrations.Any(r => r.RegistrationId == registrationId && r.EventId == id);
        if (ev.Status == "Cancelled" || !belongs)
        {
            TempData["Error"] = "Attendance cannot be changed for this registration.";
        }
        else
        {
            var result = _registrations.SetAttendance(registrationId, UserId, attended);
            TempData[result.Ok ? "Success" : "Error"] = result.Message;
        }
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostComplete(int id)
    {
        var ev = _db.Events.Find(id);
        if (ev == null) return NotFound();
        if (!CanManage(ev)) return RedirectToPage("/AccessDenied");

        if (ev.Status == "Active")
        {
            ev.Status = "Completed";
            _db.SaveChanges();
            TempData["Success"] = "Event marked as completed.";
        }
        return RedirectToPage(new { id });
    }
}
