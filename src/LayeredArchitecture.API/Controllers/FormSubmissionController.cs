using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormSubmissionController : ControllerBase
{
    private readonly IFormSubmissionService _formSubmissionService;
    private readonly ILogger<FormSubmissionController> _logger;

    public FormSubmissionController(
        IFormSubmissionService formSubmissionService,
        ILogger<FormSubmissionController> logger)
    {
        _formSubmissionService = formSubmissionService;
        _logger = logger;
    }

    /// <summary>
    /// Submit FTE or CW request with approval workflow
    /// </summary>
    /// <param name="request">Form submission request containing request details and approvals</param>
    /// <returns>Response with status, request ID, or error details</returns>
    /// <response code="200">Request submitted successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(FormSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FormSubmissionResponse>> SubmitRequest([FromBody] FormSubmissionRequest request)
    {
        try
        {
            _logger.LogInformation("Received {RequestType} request from {Email}",
                request?.Request?.RequestType, request?.Request?.RequestedByEmail);

            var response = await _formSubmissionService.SubmitRequestAsync(request);

            if (response.Status == "success")
            {
                _logger.LogInformation("Successfully processed request. RequestId: {RequestId}", response.RequestId);
                return Ok(response);
            }
            else
            {
                _logger.LogError("Request failed: {Error}, Failed step: {FailedStep}",
                    response.Error, response.FailedStep);
                return StatusCode(500, response);
            }
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Null argument in request");
            return BadRequest(new FormSubmissionResponse
            {
                Status = "error",
                Error = ex.Message,
                FailedStep = "Validation"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in request");
            return BadRequest(new FormSubmissionResponse
            {
                Status = "error",
                Error = ex.Message,
                FailedStep = "Validation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing request");
            return StatusCode(500, new FormSubmissionResponse
            {
                Status = "error",
                Error = "An unexpected error occurred",
                FailedStep = "Server Error"
            });
        }
    }

    /// <summary>
    /// Update existing FTE or CW request with approval workflow
    /// </summary>
    /// <param name="requestId">The ID of the request to update</param>
    /// <param name="request">Form submission request containing updated request details and approvals</param>
    /// <returns>Response with status, request ID, or error details</returns>
    /// <response code="200">Request updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{requestId}")]
    [ProducesResponseType(typeof(FormSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FormSubmissionResponse>> UpdateRequest(
        [FromRoute] string requestId,
        [FromBody] FormSubmissionRequest request)
    {
        try
        {
            _logger.LogInformation("Received update request for RequestId: {RequestId}, RequestType: {RequestType}",
                requestId, request?.Request?.RequestType);

            // Validate requestId format
            if (!Guid.TryParse(requestId, out _))
            {
                return BadRequest(new FormSubmissionResponse
                {
                    Status = "error",
                    Error = "Invalid RequestId format. Must be a valid GUID.",
                    FailedStep = "Validation"
                });
            }

            var response = await _formSubmissionService.UpdateRequestAsync(requestId, request);

            if (response.Status == "success")
            {
                _logger.LogInformation("Successfully updated request. RequestId: {RequestId}", response.RequestId);
                return Ok(response);
            }
            else if (response.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Request not found: {RequestId}", requestId);
                return NotFound(response);
            }
            else
            {
                _logger.LogError("Request update failed: {Error}, Failed step: {FailedStep}",
                    response.Error, response.FailedStep);
                return StatusCode(500, response);
            }
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Null argument in update request");
            return BadRequest(new FormSubmissionResponse
            {
                Status = "error",
                Error = ex.Message,
                FailedStep = "Validation"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in update request");
            return BadRequest(new FormSubmissionResponse
            {
                Status = "error",
                Error = ex.Message,
                FailedStep = "Validation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating request");
            return StatusCode(500, new FormSubmissionResponse
            {
                Status = "error",
                Error = "An unexpected error occurred",
                FailedStep = "Server Error"
            });
        }
    }
}
