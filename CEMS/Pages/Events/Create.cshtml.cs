using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Events;

[SessionAuthorize(Roles = "Staff,Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly EventResourceService _resources;

    public CreateModel(AppDbContext db, EventResourceService resources)
    {
        _db = db;
        _resources = resources;
    }

    [BindProperty]
    public EventInput Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> Files { get; set; } = new();

    public void OnGet()
    {
        Input.EventDate = DateTime.Today.AddDays(7).AddHours(9);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EventDate <= DateTime.Now)
            ModelState.AddModelError("Input.EventDate", "The event date must be in the future.");

        var fileError = _resources.Validate(Files);
        if (fileError != null)
            ModelState.AddModelError("Files", fileError);

        if (!ModelState.IsValid) return Page();

        var ev = new Event
        {
            Title = Input.Title.Trim(),
            Description = Input.Description.Trim(),
            EventDate = Input.EventDate,
            Venue = Input.Venue.Trim(),
            Category = Input.Category,
            MaxSeats = Input.MaxSeats,
            OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value
        };
        _db.Events.Add(ev);
        _db.SaveChanges();

        await _resources.SaveAsync(ev.EventId, Files);

        TempData["Success"] = "Event created successfully!";
        return RedirectToPage("Detail", new { id = ev.EventId });
    }
}
