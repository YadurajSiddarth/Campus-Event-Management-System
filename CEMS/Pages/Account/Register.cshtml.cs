using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CEMS.Data;
using CEMS.Models;
using CEMS.Services;

namespace CEMS.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly AppDbContext _db;
    public RegisterModel(AppDbContext db) => _db = db;

    public void OnGet() { }

    public IActionResult OnPost(string fullName, string email, string password, string confirmPassword, string? department, string? contactNumber)
    {
        fullName = (fullName ?? "").Trim();
        email = (email ?? "").Trim();
        department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
        contactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber.Trim();

        string? error = null;
        if (fullName.Length < 2 || fullName.Length > 100) error = "Please enter your full name (2-100 characters).";
        else if (!IsValidEmail(email) || email.Length > 150) error = "Please enter a valid email address.";
        else if (PasswordPolicy.Validate(password) is { } pwError) error = pwError;
        else if (password != confirmPassword) error = "Passwords do not match.";
        else if (contactNumber != null && !Regex.IsMatch(contactNumber, @"^[0-9+\- ]{8,20}$")) error = "Contact number may contain digits, spaces, + and - only (8-20 characters).";
        else if (department != null && department.Length > 100) error = "Department is too long.";
        else if (_db.Users.Any(u => u.Email == email)) error = "That email address is already registered.";

        if (error != null)
        {
            TempData["Error"] = error;
            return Page();
        }

        _db.Users.Add(new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Department = department,
            ContactNumber = contactNumber,
            Role = "Student"
        });
        _db.SaveChanges();

        TempData["Success"] = "Registration successful! Please login.";
        return RedirectToPage("Login");
    }

    internal static bool IsValidEmail(string email)
    {
        try { return new MailAddress(email).Address == email; }
        catch (FormatException) { return false; }
    }
}
