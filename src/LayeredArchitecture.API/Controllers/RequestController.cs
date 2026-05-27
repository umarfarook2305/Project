using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Request operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RequestController : ControllerBase
{
    private readonly IResubmitRequestService _resubmitRequestService;
    private readonly ILogger<RequestController> _logger;

    public RequestController(
        IResubmitRequestService resubmitRequestService,
        ILogger<RequestController> logger)
    {
        _resubmitRequestService = resubmitRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Resubmit a request with updated data and new approvals
    /// </summary>
    [HttpPut("Resubmit")]
    [ProducesResponseType(typeof(ResubmitResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ResubmitRequest([FromBody] ResubmitRequest request)
    {
        try
        {
            var result = await _resubmitRequestService.ResubmitRequestAsync(request);

            if (result.StatusCode == "404")
                return NotFound(result);

            if (result.StatusCode != "200")
                return StatusCode(int.Parse(result.StatusCode), result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResubmitResponse
            {
                StatusCode = "400",
                Body = new ResubmitBody { Status = "error", Message = ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resubmitting request");
            return StatusCode(500, new ResubmitResponse
            {
                StatusCode = "500",
                Body = new ResubmitBody { Status = "error", Message = "Failed to resubmit request" }
            });
        }
    }
}
