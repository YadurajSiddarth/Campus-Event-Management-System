using System.ComponentModel.DataAnnotations;

namespace CEMS.Models;

/// <summary>Form model shared by the Create Event and Edit Event pages.</summary>
public class EventInput : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = "";

    [Required, Display(Name = "Date & Time")]
    public DateTime EventDate { get; set; }

    [Required, StringLength(200)]
    public string Venue { get; set; } = "";

    [Required]
    public string Category { get; set; } = "Seminar";

    [Required, Range(1, 10000), Display(Name = "Maximum Seats")]
    public int MaxSeats { get; set; } = 50;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!EventCategories.All.Contains(Category))
            yield return new ValidationResult("Please choose a valid category.", new[] { nameof(Category) });
    }
}
