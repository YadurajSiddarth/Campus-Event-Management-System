using CEMS.Data;
using CEMS.Models;

namespace CEMS.Services;

/// <summary>Validates, stores and removes event brochures/resources (saved under wwwroot/uploads/events).</summary>
public class EventResourceService
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EventResourceService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    /// <summary>Returns an error message, or null when all files are acceptable.</summary>
    public string? Validate(IEnumerable<IFormFile> files)
    {
        foreach (var f in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return $"'{f.FileName}' is not allowed. Upload PDF, Word or image files only.";
            if (f.Length > MaxBytes)
                return $"'{f.FileName}' is larger than 5 MB.";
        }
        return null;
    }

    public async Task SaveAsync(int eventId, IEnumerable<IFormFile> files)
    {
        var folder = Path.Combine(_env.WebRootPath, "uploads", "events", eventId.ToString());
        Directory.CreateDirectory(folder);

        foreach (var f in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid():N}{ext}";
            await using (var stream = File.Create(Path.Combine(folder, storedName)))
            {
                await f.CopyToAsync(stream);
            }

            var displayName = Path.GetFileName(f.FileName);
            if (displayName.Length > 200) displayName = displayName[^200..];
            _db.EventResources.Add(new EventResource
            {
                EventId = eventId,
                FileName = displayName,
                FilePath = $"/uploads/events/{eventId}/{storedName}"
            });
        }
        await _db.SaveChangesAsync();
    }

    public void Delete(int resourceId, int eventId)
    {
        var res = _db.EventResources.FirstOrDefault(r => r.ResourceId == resourceId && r.EventId == eventId);
        if (res == null) return;

        if (res.FilePath.StartsWith("/uploads/events/"))
        {
            var physical = Path.Combine(_env.WebRootPath, res.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physical)) File.Delete(physical);
        }
        _db.EventResources.Remove(res);
        _db.SaveChanges();
    }
}
