using System.ComponentModel.DataAnnotations;

namespace TowerFight.API.Models;

public record InsertHighscoreRequest
{
    [Required]
    public byte? Difficulty { get; init; }

    [Required]
    public int? Score { get; init; }

    [Required]
    [StringLength(32)]
    public string? Name { get; init; } 

    public string? Hash { get; init; }

    public Guid? Guid { get; init; }
}
