using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HierarchyController : ControllerBase
{
    private readonly IHierarchyService _hierarchyService;
    private readonly ILogger<HierarchyController> _logger;

    public HierarchyController(
        IHierarchyService hierarchyService,
        ILogger<HierarchyController> logger)
    {
        _hierarchyService = hierarchyService;
        _logger = logger;
    }

    /// <summary>
    /// Get employee management hierarchy by email address
    /// </summary>
    /// <param name="request">Request containing employee email</param>
    /// <returns>Management hierarchy with levels from direct manager to top-level executive</returns>
    /// <response code="200">Returns the management hierarchy</response>
    /// <response code="400">If email is missing or invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(HierarchyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HierarchyResponse>> GetHierarchy([FromBody] HierarchyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                _logger.LogWarning("Hierarchy request received with empty email");
                return BadRequest(new { status = "error", message = "Email is required" });
            }

            _logger.LogInformation("Getting hierarchy for email: {Email}", request.Email);

            var hierarchy = await _hierarchyService.GetHierarchyAsync(request.Email);

            return Ok(hierarchy);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request: {Message}", ex.Message);
            return BadRequest(new { status = "error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hierarchy for email: {Email}", request?.Email);
            return StatusCode(500, new 
            { 
                status = "error", 
                message = "An error occurred while retrieving hierarchy" 
            });
        }
    }
}
