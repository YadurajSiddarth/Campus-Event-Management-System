using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMS.Models;

public class Registration
{
    [Key]
    public int RegistrationId { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event Event { get; set; } = null!;

    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Status { get; set; } = "Registered";

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    /// <summary>Optional note from the participant (e.g. dietary or accessibility needs). Editable while registered.</summary>
    [MaxLength(200)]
    public string? Remarks { get; set; }

    public ICollection<RegistrationLog> Logs { get; set; } = new List<RegistrationLog>();
}
