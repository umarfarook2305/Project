using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IRequestFilterService _service;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IRequestFilterService service, ILogger<RequestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of requests with optional filtering
    /// </summary>
    /// <param name="filter">Filter parameters including pagination</param>
    /// <returns>Paginated list of requests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RequestListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<RequestListItemResponse>>> GetRequests([FromQuery] RequestFilterRequest filter)
    {
        try
        {
            _logger.LogInformation("Getting request list with filters: PageNumber={PageNumber}, PageSize={PageSize}, Type={Type}, ViewType={ViewType}",
                filter.PageNumber, filter.PageSize, filter.Type, filter.ViewType);

            var result = await _service.GetRequestListAsync(filter);

            _logger.LogInformation("Successfully retrieved {Count} requests out of {Total} total records",
                result.Data.Count, result.Pagination.TotalRecords);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new
            {
                error = "Invalid request parameters",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request list");
            return StatusCode(500, new
            {
                error = "An error occurred while retrieving requests",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get detailed request information by ID
    /// </summary>
    /// <param name="requestId">Request GUID</param>
    /// <param name="type">Optional request type (FTE or CW)</param>
    /// <returns>Detailed request information</returns>
    [HttpGet("{requestId}")]
    [ProducesResponseType(typeof(RequestDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RequestDetailResponse>> GetRequestDetail(string requestId, [FromQuery] string? type = null)
    {
        try
        {
            _logger.LogInformation("Getting request detail for RequestId={RequestId}, Type={Type}",
                requestId, type);

            var result = await _service.GetRequestDetailAsync(requestId, type);

            if (result == null)
            {
                _logger.LogWarning("Request not found: RequestId={RequestId}", requestId);
                return NotFound(new
                {
                    error = "Request not found",
                    requestId
                });
            }

            _logger.LogInformation("Successfully retrieved request detail for RequestId={RequestId}", requestId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new
            {
                error = "Invalid request parameters",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request detail for RequestId={RequestId}", requestId);
            return StatusCode(500, new
            {
                error = "An error occurred while retrieving request detail",
                message = ex.Message
            });
        }
    }
}
