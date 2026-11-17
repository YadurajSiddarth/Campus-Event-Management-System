using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Models;

namespace CEMS.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Event> UpcomingEvents { get; set; } = new();

    public void OnGet()
    {
        UpcomingEvents = _db.Events
            .Include(e => e.Registrations)
            .Where(e => e.Status == "Active" && e.EventDate > DateTime.Now)
            .OrderBy(e => e.EventDate)
            .Take(6)
            .ToList();
    }
}
