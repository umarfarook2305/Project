using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobProfileController : ControllerBase
{
    private readonly IJobProfileService _jobProfileService;
    private readonly ILogger<JobProfileController> _logger;

    public JobProfileController(
        IJobProfileService jobProfileService,
        ILogger<JobProfileController> logger)
    {
        _jobProfileService = jobProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Get job profiles and families
    /// </summary>
    /// <param name="request">Request containing optional JobLevel filter</param>
    /// <returns>List of job profiles with their families</returns>
    /// <response code="200">Returns the list of job profiles</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(List<JobProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<JobProfileResponse>>> GetJobProfiles([FromBody] JobProfileRequest? request)
    {
        try
        {
            var jobLevel = request?.JobLevel;
            _logger.LogInformation("Getting job profiles with filter: {JobLevel}", jobLevel ?? "All");

            var profiles = await _jobProfileService.GetJobProfilesAsync(jobLevel);

            return Ok(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job profiles");
            return StatusCode(500, new { error = "An error occurred while retrieving job profiles" });
        }
    }
}
