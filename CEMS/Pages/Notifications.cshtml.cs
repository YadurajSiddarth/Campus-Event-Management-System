using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;

namespace CEMS.Pages;

[SessionAuthorize]
public class NotificationsModel : PageModel
{
    private readonly AppDbContext _db;
    public NotificationsModel(AppDbContext db) => _db = db;

    public List<Notification> Items { get; set; } = new();

    public void OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        Items = _db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToList();
    }

    public IActionResult OnPost()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        foreach (var n in _db.Notifications.Where(n => n.UserId == userId && !n.IsRead))
            n.IsRead = true;
        _db.SaveChanges();
        return RedirectToPage();
    }
}
