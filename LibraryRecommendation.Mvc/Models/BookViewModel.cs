using System.ComponentModel.DataAnnotations;

namespace LibraryRecommendation.Mvc.Models;

// Deliberately a separate DTO from the API entity: the MVC project must not reference
// EF Core or the API project. PROVISIONAL — mirror whatever the brief specifies.
public class BookViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    [MaxLength(2000)]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    [Display(Name = "Published Year")]
    [Range(1000, 2100)]
    public int PublishedYear { get; set; }
}
