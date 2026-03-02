namespace TowerFight.BusinessLogic.Data.Config;

public record DbSettings
{
    public string PgConnectionString { get; init; }
}