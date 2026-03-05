using Riok.Mapperly.Abstractions;

namespace TowerFight.BusinessLogic.Models;

public record Leader
{
    [MapperIgnore]
    public int Number { get; init; }
    public byte Difficulty { get; init;}
    public int Score { get; init; }
    public string Name { get; init; }
}