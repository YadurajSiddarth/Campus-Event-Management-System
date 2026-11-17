using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMS.Models;

/// <summary>Audit trail entry: one row for every change made to a registration.</summary>
public class RegistrationLog
{
    [Key]
    public int LogId { get; set; }

    public int RegistrationId { get; set; }

    [ForeignKey("RegistrationId")]
    public Registration Registration { get; set; } = null!;

    /// <summary>The user who made the change (the participant, an organiser or an admin).</summary>
    public int ActorUserId { get; set; }

    [ForeignKey("ActorUserId")]
    public User Actor { get; set; } = null!;

    [Required, MaxLength(30)]
    public string Action { get; set; } = "";

    [MaxLength(500)]
    public string? Details { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.Now;
}
