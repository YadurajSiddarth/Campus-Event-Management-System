using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMS.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Venue { get; set; } = "";

    [Required, MaxLength(50)]
    public string Category { get; set; } = "";

    [Required, Range(1, 10000)]
    public int MaxSeats { get; set; }

    public int OrganiserId { get; set; }

    [ForeignKey("OrganiserId")]
    public User Organiser { get; set; } = null!;

    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Seats currently taken (every registration that has not been cancelled).</summary>
    [NotMapped]
    public int SeatsTaken => Registrations.Count(r => r.Status != "Cancelled");

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
    public ICollection<EventResource> Resources { get; set; } = new List<EventResource>();
}
