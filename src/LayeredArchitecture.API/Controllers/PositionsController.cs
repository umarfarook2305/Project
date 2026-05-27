using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Position operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(
        IPositionService positionService,
        ILogger<PositionsController> logger)
    {
        _positionService = positionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all positions with user details and job requisitions
    /// </summary>
    /// <remarks>
    /// Returns a list of positions including:
    /// - Employee details (name, GUID)
    /// - Position information (ID, GUID, status)
    /// - Job requisition details (ID, GUID, profile name)
    /// 
    /// Equivalent to Power Platform API: PositionId (GET)
    /// </remarks>
    /// <response code="200">Success - Returns position data</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PositionResponse>> GetPositions()
    {
        try
        {
            _logger.LogInformation("Getting all positions");

            var result = await _positionService.GetPositionsAsync();

            _logger.LogInformation("Successfully retrieved {Count} positions", result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving positions");

            return StatusCode(500, new PositionResponse
            {
                Status = "error",
                Count = 0,
                Data = new List<PositionData>()
            });
        }
    }
}
