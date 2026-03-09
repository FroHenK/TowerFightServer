using System.Security.Cryptography;
using System.Text;
using TowerFight.API.Models;
using TowerFight.BusinessLogic.Data.Config;

namespace TowerFight.API.Utilities;

public static class HighscoreHashUtility
{
    //private const string Salt = "359556b3-c70b-4106-8dff-4e723bda8cfc";

    private static readonly string Salt;

    static HighscoreHashUtility()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("secrets.json")
            .Build();
        var signingSettings = configuration.GetSection(nameof(SigningSettings)).Get<SigningSettings>()!;

        Salt = signingSettings?.HighscoreHashSalt ?? throw new InvalidOperationException("HighscoreHashSalt not configured");
    }

    public static bool IsValid(InsertHighscoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Hash))
        {
            return false;
        }

        var parts = new List<string>
        {
            $"{nameof(request.Difficulty)}={request.Difficulty}",
            $"{nameof(request.Score)}={request.Score}",
            $"{nameof(request.Name)}={request.Name}",
            $"{nameof(Salt)}={Salt}",
        };
        
        if (request.Guid.HasValue)
        {
            parts.Add($"{nameof(request.Guid)}={request.Guid}");
        }
        
        parts.Sort();
        var payload = string.Join(":", parts);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hashBytes = SHA256.HashData(payloadBytes);
        var expectedHash = Convert.ToHexString(hashBytes);

        return string.Equals(expectedHash, request.Hash, StringComparison.OrdinalIgnoreCase);
    }
}
