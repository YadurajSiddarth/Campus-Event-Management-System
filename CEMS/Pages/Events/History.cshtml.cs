using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Events;

[SessionAuthorize]
public class HistoryModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly RegistrationService _registrations;

    public HistoryModel(AppDbContext db, RegistrationService registrations)
    {
        _db = db;
        _registrations = registrations;
    }

    public List<Registration> Registrations { get; set; } = new();

    private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;

    public void OnGet()
    {
        Registrations = _db.Registrations
            .Include(r => r.Event)
            .Where(r => r.UserId == UserId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToList();
    }

    public IActionResult OnPostCancel(int registrationId, string? reason)
    {
        var result = _registrations.Cancel(registrationId, UserId, reason);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return RedirectToPage();
    }

    public IActionResult OnPostUpdate(int registrationId, string? remarks)
    {
        var result = _registrations.UpdateRemarks(registrationId, UserId, remarks);
        TempData[result.Ok ? "Success" : "Error"] = result.Message;
        return RedirectToPage();
    }
}
