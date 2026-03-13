using System.Security.Cryptography;
using System.Text;
using TowerFight.API.Models;
using TowerFight.BusinessLogic.Data.Config;

namespace TowerFight.API.Utilities;

public class HighscoreHashUtility
{
    private readonly string _salt;

    private readonly ILogger<HighscoreHashUtility> _logger;

    public HighscoreHashUtility(IConfiguration configuration, ILogger<HighscoreHashUtility> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var signingSettings = configuration.GetSection("SigningSettings").Get<SigningSettings>();
        _salt = signingSettings?.HighscoreHashSalt ?? throw new InvalidOperationException("HighscoreHashSalt not configured");
        _logger.LogInformation("HighscoreHashUtility initialized with provided configuration.");
    }

    public bool IsValid(InsertHighscoreRequest request)
    {
        _logger.LogInformation("Validating highscore request for Name: {Name}, Score: {Score}, Difficulty: {Difficulty}, Guid: {Guid}", request.Name, request.Score, request.Difficulty, request.Guid);

        if (string.IsNullOrWhiteSpace(request.Hash))
        {
            _logger.LogWarning("Request hash is null or whitespace.");
            return false;
        }

        var parts = new List<string>
        {
            $"{nameof(request.Difficulty)}={request.Difficulty}",
            $"{nameof(request.Score)}={request.Score}",
            $"{nameof(request.Name)}={request.Name}",
            $"Salt={_salt}",
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

        bool isValid = string.Equals(expectedHash, request.Hash, StringComparison.OrdinalIgnoreCase);

        if (isValid)
        {
            _logger.LogInformation("Highscore request is valid.");
        }
        else
        {
            _logger.LogWarning("Highscore request is invalid. Expected hash: {ExpectedHash}, Provided hash: {ProvidedHash}", expectedHash, request.Hash);
        }

        return isValid;
    }
}
