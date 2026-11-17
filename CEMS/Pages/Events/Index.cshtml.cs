using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Models;

namespace CEMS.Pages.Events;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Event> Events { get; set; } = new();
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Organiser { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public void OnGet(string? search, string? category, string? organiser, DateTime? dateFrom, DateTime? dateTo)
    {
        Search = search;
        Category = category;
        Organiser = organiser;
        DateFrom = dateFrom;
        DateTo = dateTo;

        var query = _db.Events.Include(e => e.Registrations).Include(e => e.Organiser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search) || e.Venue.Contains(search));
        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);
        if (!string.IsNullOrWhiteSpace(organiser))
            query = query.Where(e => e.Organiser.FullName.Contains(organiser));
        if (dateFrom.HasValue)
            query = query.Where(e => e.EventDate >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(e => e.EventDate < dateTo.Value.Date.AddDays(1));

        Events = query.OrderBy(e => e.EventDate).ToList();
    }
}
