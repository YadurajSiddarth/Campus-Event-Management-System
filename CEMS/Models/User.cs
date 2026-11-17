using System.ComponentModel.DataAnnotations;

namespace CEMS.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(150)]
    public string Email { get; set; } = "";

    [Required, MaxLength(256)]
    public string PasswordHash { get; set; } = "";

    [MaxLength(20)]
    public string? ContactNumber { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Student";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
