using System.ComponentModel.DataAnnotations;

namespace TowerFight.BusinessLogic.Data.Models;

public record LeaderDao 
{
    [Key]
    public int Id { get; init; }
    public int Number { get; init; }
    public Guid Guid { get; init; }
    [StringLength(32)]
    public string Name { get; init; }
    public byte Difficulty { get; init;}
}