using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Services;

namespace CEMS.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    public LoginModel(AppDbContext db) => _db = db;

    public void OnGet() { }

    public IActionResult OnPost(string email, string password)
    {
        email = (email ?? "").Trim();
        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !PasswordHasher.Verify(password ?? "", user.PasswordHash))
        {
            TempData["Error"] = "Invalid email or password.";
            return Page();
        }

        // Start a clean session for the logged-in user
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserEmail", user.Email);
        TempData["Success"] = $"Welcome back, {user.FullName}!";
        return RedirectToPage("/Index");
    }
}
