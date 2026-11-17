using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;

namespace CEMS.Pages.Events;

[SessionAuthorize]
public class LogsModel : PageModel
{
    private readonly AppDbContext _db;
    public LogsModel(AppDbContext db) => _db = db;

    public List<RegistrationLog> Logs { get; set; } = new();
    public string Scope { get; set; } = "";

    public void OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var role = HttpContext.Session.GetString("UserRole");

        var query = _db.RegistrationLogs
            .Include(l => l.Actor)
            .Include(l => l.Registration).ThenInclude(r => r.Event)
            .Include(l => l.Registration).ThenInclude(r => r.User)
            .AsQueryable();

        if (role == "Admin")
        {
            Scope = "All registration changes in the system";
        }
        else if (role == "Staff")
        {
            query = query.Where(l => l.Registration.Event.OrganiserId == userId || l.Registration.UserId == userId);
            Scope = "Changes to registrations for your events and your own registrations";
        }
        else
        {
            query = query.Where(l => l.Registration.UserId == userId);
            Scope = "Changes to your own registrations";
        }

        Logs = query.OrderByDescending(l => l.LoggedAt).Take(200).ToList();
    }
}
