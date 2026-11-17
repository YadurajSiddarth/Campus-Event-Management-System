using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages;

[SessionAuthorize]
public class EnquiriesModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notify;

    public EnquiriesModel(AppDbContext db, NotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public List<Enquiry> Sent { get; set; } = new();
    public List<Enquiry> Received { get; set; } = new();
    public bool IsOrganiser { get; set; }

    private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;
    private string Role => HttpContext.Session.GetString("UserRole") ?? "";

    public void OnGet()
    {
        IsOrganiser = Role == "Staff" || Role == "Admin";

        Sent = _db.Enquiries.Include(q => q.Event)
            .Where(q => q.UserId == UserId)
            .OrderByDescending(q => q.SentAt).ToList();

        if (IsOrganiser)
        {
            var query = _db.Enquiries.Include(q => q.Event).Include(q => q.User).AsQueryable();
            if (Role != "Admin")
                query = query.Where(q => q.Event.OrganiserId == UserId);
            Received = query.OrderBy(q => q.Reply != null).ThenByDescending(q => q.SentAt).ToList();
        }
    }

    public IActionResult OnPostReply(int enquiryId, string reply)
    {
        if (Role != "Staff" && Role != "Admin") return RedirectToPage("/AccessDenied");

        var enquiry = _db.Enquiries.Include(q => q.Event).FirstOrDefault(q => q.EnquiryId == enquiryId);
        if (enquiry == null)
        {
            TempData["Error"] = "Enquiry not found.";
            return RedirectToPage();
        }
        if (Role != "Admin" && enquiry.Event.OrganiserId != UserId) return RedirectToPage("/AccessDenied");

        reply = (reply ?? "").Trim();
        if (reply.Length == 0 || reply.Length > 1000)
        {
            TempData["Error"] = "Please enter a reply (up to 1000 characters).";
            return RedirectToPage();
        }

        enquiry.Reply = reply;
        enquiry.RepliedAt = DateTime.Now;
        _db.SaveChanges();

        _notify.Notify(enquiry.UserId,
            $"The organiser replied to your enquiry \"{enquiry.Subject}\" about {enquiry.Event.Title}: {reply}",
            "Reply to your enquiry");
        TempData["Success"] = "Reply sent to the participant.";
        return RedirectToPage();
    }
}
