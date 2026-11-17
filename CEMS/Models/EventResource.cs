using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMS.Models;

public class EventResource
{
    [Key]
    public int ResourceId { get; set; }

    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event Event { get; set; } = null!;

    [Required, MaxLength(200)]
    public string FileName { get; set; } = "";

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = "";

    public DateTime UploadedAt { get; set; } = DateTime.Now;
}
