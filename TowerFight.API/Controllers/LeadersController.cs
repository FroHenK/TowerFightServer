using TowerFight.BusinessLogic.Models;
using TowerFight.BusinessLogic.Services;
using TowerFight.API.Models;
using TowerFight.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using TowerFight.BusinessLogic.Data.Config;
using System.Net;

namespace TowerFight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadersController(
    ILeadersService _LeadersService,
    IConfiguration configuration,
    HighscoreHashUtility _highscoreHashUtility,
    ILogger<LeadersController> _logger) : ControllerBase
{
    [HttpGet("", Name = "GetLeaders")]
    [ProducesResponseType(typeof(IEnumerable<Leader>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeadersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetLeadersAsync called");
        var leaders = await _LeadersService.GetLeadersAsync(cancellationToken);
        _logger.LogInformation("GetLeadersAsync returning {Count} leaders", leaders?.Count() ?? 0);
        return Ok(leaders);
    }

    [HttpPost("", Name = "InsertHighscore")]
    [ProducesResponseType(typeof(InsertHighscoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InsertHighscoreAsync(
        [FromBody] InsertHighscoreRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("InsertHighscoreAsync called with Name: {Name}, Score: {Score}, Difficulty: {Difficulty}, Guid: {Guid}",
            request.Name, request.Score, request.Difficulty, request.Guid);

        var signingSettings = configuration.GetSection(nameof(SigningSettings)).Get<SigningSettings>()!;
        if (signingSettings.Enabled && !_highscoreHashUtility.IsValid(request))
        {
            _logger.LogWarning("InsertHighscoreAsync: Invalid request signature for Guid: {Guid}", request.Guid);
            return Problem("Invalid request. Please update your app", statusCode: (int)HttpStatusCode.BadRequest);
        }

        var leader = new Leader
        {
            Difficulty = request.Difficulty!.Value,
            Score = request.Score!.Value,
            Name = request.Name!
        };

        var result = await _LeadersService.InsertHighscoreAsync(
            leader,
            request.Guid,
            cancellationToken);

        var actionResult = result.Match<IActionResult>(
            success =>
            {
                _logger.LogInformation("InsertHighscoreAsync: Highscore inserted for Guid: {Guid}", success.Guid);
                return Ok(new InsertHighscoreResponse(success.Guid));
            },
            nameError =>
            {
                _logger.LogWarning("InsertHighscoreAsync: Name conflict for Guid: {Guid} - {Reason}", request.Guid, nameError.Reason);
                return Problem(nameError.Reason, statusCode: (int)HttpStatusCode.Conflict);
            },
            noChanges =>
            {
                _logger.LogInformation("InsertHighscoreAsync: No changes for Guid: {Guid} - {Reason}", request.Guid, noChanges.Reason);
                return Accepted(string.Empty, noChanges.Reason);
            }
        );

        return actionResult;
    }

    public record InsertHighscoreResponse(Guid Guid);
}
