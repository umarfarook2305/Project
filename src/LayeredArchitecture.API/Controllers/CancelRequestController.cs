using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Cancel Request operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CancelRequestController : ControllerBase
{
    private readonly ICancelRequestService _cancelRequestService;
    private readonly ILogger<CancelRequestController> _logger;

    public CancelRequestController(
        ICancelRequestService cancelRequestService,
        ILogger<CancelRequestController> logger)
    {
        _cancelRequestService = cancelRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Cancel a position request
    /// </summary>
    /// <remarks>
    /// Cancels a position request by:
    /// - Updating all pending approvals to cancelled status
    /// - Setting the request status to cancelled
    /// - Sending notifications to all pending approvers
    /// 
    /// Equivalent to Power Platform API: postCancelRequest (POST)
    /// </remarks>
    /// <param name="request">Cancel request details</param>
    /// <response code="200">Success - Request cancelled and notifications sent</response>
    /// <response code="400">Bad Request - Invalid input</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CancelRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CancelRequestResponse>> CancelRequest([FromBody] CancelRequestDto request)
    {
        try
        {
            _logger.LogInformation(
                "Cancelling request {RequestID} by {RequestorName} ({RequestorEmail})",
                request.RequestID,
                request.RequestorName,
                request.RequestorEmail);

            var result = await _cancelRequestService.CancelRequestAsync(request);

            _logger.LogInformation(
                "Successfully cancelled request {RequestID}",
                request.RequestID);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for cancel request");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling request {RequestID}", request.RequestID);

            return StatusCode(500, new CancelRequestResponse
            {
                Message = "Error In Updating Table and Notify users"
            });
        }
    }
}
