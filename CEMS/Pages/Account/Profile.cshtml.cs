using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Account;

[SessionAuthorize]
public class ProfileModel : PageModel
{
    private readonly AppDbContext _db;
    public ProfileModel(AppDbContext db) => _db = db;

    public User UserProfile { get; set; } = null!;

    public void OnGet()
    {
        UserProfile = _db.Users.Find(HttpContext.Session.GetInt32("UserId")!.Value)!;
    }

    public IActionResult OnPostProfile(string fullName, string email, string? department, string? contactNumber)
    {
        var user = _db.Users.Find(HttpContext.Session.GetInt32("UserId")!.Value)!;
        fullName = (fullName ?? "").Trim();
        email = (email ?? "").Trim();
        department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
        contactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber.Trim();

        string? error = null;
        if (fullName.Length < 2 || fullName.Length > 100) error = "Please enter your full name (2-100 characters).";
        else if (!RegisterModel.IsValidEmail(email) || email.Length > 150) error = "Please enter a valid email address.";
        else if (_db.Users.Any(u => u.Email == email && u.UserId != user.UserId)) error = "That email address is used by another account.";
        else if (contactNumber != null && !Regex.IsMatch(contactNumber, @"^[0-9+\- ]{8,20}$")) error = "Contact number may contain digits, spaces, + and - only (8-20 characters).";
        else if (department != null && department.Length > 100) error = "Department is too long.";

        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToPage();
        }

        user.FullName = fullName;
        user.Email = email;
        user.Department = department;
        user.ContactNumber = contactNumber;
        _db.SaveChanges();

        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("UserEmail", user.Email);
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToPage();
    }

    public IActionResult OnPostPassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var user = _db.Users.Find(HttpContext.Session.GetInt32("UserId")!.Value)!;

        if (!PasswordHasher.Verify(currentPassword ?? "", user.PasswordHash))
            TempData["Error"] = "Your current password is incorrect.";
        else if (PasswordPolicy.Validate(newPassword) is { } pwError)
            TempData["Error"] = pwError;
        else if (newPassword != confirmPassword)
            TempData["Error"] = "The new passwords do not match.";
        else if (newPassword == currentPassword)
            TempData["Error"] = "The new password must be different from the current one.";
        else
        {
            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _db.SaveChanges();
            TempData["Success"] = "Password changed successfully.";
        }
        return RedirectToPage();
    }
}
