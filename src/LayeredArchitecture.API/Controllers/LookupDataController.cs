using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupDataController : ControllerBase
{
    private readonly ILookupDataService _lookupDataService;
    private readonly ILogger<LookupDataController> _logger;

    public LookupDataController(ILookupDataService lookupDataService, ILogger<LookupDataController> logger)
    {
        _lookupDataService = lookupDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get lookup data based on user type
    /// </summary>
    /// <param name="request">Request containing the user type (CW or FTE)</param>
    /// <returns>
    /// CW: Returns LineOfBusiness, ProjectTheme, SWPRole, FundingType, CountryList, Status (6 categories)
    /// FTE: Returns Joblevel, SWPRole, PositionType, FundingType, RoleType, CountryList, Status, StatusOfPosition (8 categories)
    /// </returns>
    /// <response code="200">Returns the lookup data</response>
    /// <response code="400">If the user type is invalid or missing (only CW and FTE are allowed)</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    public async Task<ActionResult<object>> GetLookupData([FromBody] LookupDataRequest request)
    {
        try
        {
            var lookupData = await _lookupDataService.GetLookupDataAsync(request?.Type ?? "");
            return Ok(lookupData);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid user type provided: {Type}", request?.Type);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lookup data for type {Type}", request?.Type);
            return StatusCode(500, new { error = "An error occurred while retrieving lookup data" });
        }
    }
}
