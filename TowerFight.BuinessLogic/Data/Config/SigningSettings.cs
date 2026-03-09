namespace TowerFight.BusinessLogic.Data.Config;

public record SigningSettings
{
    public string HighscoreHashSalt { get; init; }
    public bool Enabled { get; init; }
}