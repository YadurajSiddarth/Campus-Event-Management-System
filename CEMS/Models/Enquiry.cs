using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMS.Models;

public class Enquiry
{
    [Key]
    public int EnquiryId { get; set; }

    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event Event { get; set; } = null!;

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Subject { get; set; } = "";

    [Required, MaxLength(1000)]
    public string Message { get; set; } = "";

    [MaxLength(1000)]
    public string? Reply { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;

    public DateTime? RepliedAt { get; set; }
}
