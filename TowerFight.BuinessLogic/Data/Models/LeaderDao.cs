using System.ComponentModel.DataAnnotations;
using Riok.Mapperly.Abstractions;

namespace TowerFight.BusinessLogic.Data.Models;

public record LeaderDao 
{
    [Key]
    [MapperIgnore]
    public int Id { get; init; }
    public byte Difficulty { get; init;}
    public int Score { get; init; }
    [StringLength(32)]
    public string Name { get; init; }
    public Guid Guid { get; init; }
    [MapperIgnore]
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.UtcNow;
}