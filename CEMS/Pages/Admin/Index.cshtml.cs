using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;

namespace CEMS.Pages.Admin;

[SessionAuthorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private static readonly string[] Roles = { "Student", "Staff", "Admin" };

    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<User> Users { get; set; } = new();
    public int CurrentUserId { get; set; }

    public void OnGet()
    {
        CurrentUserId = HttpContext.Session.GetInt32("UserId")!.Value;
        Users = _db.Users.OrderBy(u => u.UserId).ToList();
    }

    public IActionResult OnPost(int userId, string role)
    {
        var currentId = HttpContext.Session.GetInt32("UserId");
        var user = _db.Users.Find(userId);

        if (user == null) TempData["Error"] = "User not found.";
        else if (!Roles.Contains(role)) TempData["Error"] = "Invalid role.";
        else if (userId == currentId) TempData["Error"] = "You cannot change your own role.";
        else
        {
            user.Role = role;
            _db.SaveChanges();
            TempData["Success"] = $"{user.FullName} is now a {role}.";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int userId)
    {
        var currentId = HttpContext.Session.GetInt32("UserId");
        var user = _db.Users.Find(userId);

        if (user == null) TempData["Error"] = "User not found.";
        else if (userId == currentId) TempData["Error"] = "You cannot delete your own account.";
        else if (_db.Events.Any(e => e.OrganiserId == userId)
                 || _db.Registrations.Any(r => r.UserId == userId)
                 || _db.Feedbacks.Any(f => f.UserId == userId)
                 || _db.Enquiries.Any(q => q.UserId == userId)
                 || _db.RegistrationLogs.Any(l => l.ActorUserId == userId))
            TempData["Error"] = "This user has events, registrations or other records and cannot be deleted.";
        else
        {
            _db.Notifications.RemoveRange(_db.Notifications.Where(n => n.UserId == userId));
            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["Success"] = "User deleted.";
        }
        return RedirectToPage();
    }
}
