using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;

namespace CEMS.Pages;

public record EventReportRow(
    int EventId, string Title, string Category, DateTime EventDate, string Status,
    int Capacity, int Registered, int Attended, int Cancelled,
    double AttendanceRate, double AvgRating, int FeedbackCount);

[SessionAuthorize(Roles = "Staff,Admin")]
public class ReportsModel : PageModel
{
    private readonly AppDbContext _db;
    public ReportsModel(AppDbContext db) => _db = db;

    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalAttended { get; set; }
    public int TotalUsers { get; set; }
    public double AvgRating { get; set; }
    public List<EventReportRow> Rows { get; set; } = new();
    public List<EventReportRow> Popular { get; set; } = new();
    public List<(string Category, int Registrations)> ByCategory { get; set; } = new();
    public List<Feedback> RecentFeedback { get; set; } = new();

    public void OnGet()
    {
        var events = _db.Events.Include(e => e.Registrations).Include(e => e.Feedbacks)
            .OrderByDescending(e => e.EventDate).ToList();

        Rows = events.Select(e =>
        {
            var registered = e.Registrations.Count(r => r.Status != "Cancelled");
            var attended = e.Registrations.Count(r => r.Status == "Attended");
            return new EventReportRow(
                e.EventId, e.Title, e.Category, e.EventDate, e.Status,
                e.MaxSeats, registered, attended,
                e.Registrations.Count(r => r.Status == "Cancelled"),
                registered == 0 ? 0 : 100.0 * attended / registered,
                e.Feedbacks.Any() ? e.Feedbacks.Average(f => f.Rating) : 0,
                e.Feedbacks.Count);
        }).ToList();

        TotalEvents = Rows.Count;
        TotalRegistrations = Rows.Sum(r => r.Registered);
        TotalAttended = Rows.Sum(r => r.Attended);
        TotalUsers = _db.Users.Count();
        AvgRating = _db.Feedbacks.Any() ? _db.Feedbacks.Average(f => f.Rating) : 0;

        Popular = Rows.OrderByDescending(r => r.Registered).ThenBy(r => r.Title).Take(5).ToList();
        ByCategory = Rows.GroupBy(r => r.Category)
            .Select(g => (g.Key, g.Sum(r => r.Registered)))
            .OrderByDescending(x => x.Item2).ToList();

        RecentFeedback = _db.Feedbacks.Include(f => f.Event).Include(f => f.User)
            .OrderByDescending(f => f.SubmittedAt).Take(10).ToList();
    }
}
