using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Models;

namespace CEMS.Pages.Events;

public class CalendarModel : PageModel
{
    private readonly AppDbContext _db;
    public CalendarModel(AppDbContext db) => _db = db;

    public int Year { get; set; }
    public int Month { get; set; }
    public List<Event> MonthEvents { get; set; } = new();
    public List<Event> Upcoming { get; set; } = new();

    public void OnGet(int? year, int? month)
    {
        var now = DateTime.Now;
        Year = year is >= 2000 and <= 2100 ? year.Value : now.Year;
        Month = month is >= 1 and <= 12 ? month.Value : now.Month;

        var start = new DateTime(Year, Month, 1);
        var end = start.AddMonths(1);

        MonthEvents = _db.Events.Include(e => e.Registrations)
            .Where(e => e.Status == "Active" && e.EventDate >= start && e.EventDate < end)
            .OrderBy(e => e.EventDate)
            .ToList();

        Upcoming = _db.Events.Include(e => e.Registrations)
            .Where(e => e.Status == "Active" && e.EventDate >= now)
            .OrderBy(e => e.EventDate)
            .Take(10)
            .ToList();
    }
}
