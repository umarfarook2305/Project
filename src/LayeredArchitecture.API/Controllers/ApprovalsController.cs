using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Approval List operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalListService _approvalListService;
    private readonly IApprovalDelegationListService _approvalDelegationListService;
    private readonly IApproverActionService _approverActionService;
    private readonly ILogger<ApprovalsController> _logger;

    public ApprovalsController(
        IApprovalListService approvalListService,
        IApprovalDelegationListService approvalDelegationListService,
        IApproverActionService approverActionService,
        ILogger<ApprovalsController> logger)
    {
        _approvalListService = approvalListService;
        _approvalDelegationListService = approvalDelegationListService;
        _approverActionService = approverActionService;
        _logger = logger;
    }

    /// <summary>
    /// Get approval list with comprehensive filters
    /// </summary>
    /// <remarks>
    /// Returns a list of approvals filtered by multiple criteria:
    /// - **Required**: Approver email addresses, isPending flag, count limit
    /// - **Optional**: Position type, funding type, job level, request type, pending with email
    /// - **Optional**: Requestor emails, view type (MyView/MyTeam), logged-in user email
    /// - **Optional**: Timeline (last N months or custom date range)
    /// 
    /// Combines functionality from two Power Platform APIs:
    /// - getDataList_Approval (base approval list)
    /// - getDataByFilter_Request (advanced filtering)
    /// 
    /// Sample request (basic):
    /// 
    ///     POST /api/Approvals/List
    ///     {
    ///         "listTable": "Approval",
    ///         "approver": ["ramesh.kaliyaperumal@kumaran.com"],
    ///         "isPending": true,
    ///         "count": 5000
    ///     }
    /// 
    /// Sample request (with filters):
    /// 
    ///     POST /api/Approvals/List
    ///     {
    ///         "listTable": "Approval",
    ///         "approver": ["ramesh.kaliyaperumal@kumaran.com"],
    ///         "isPending": true,
    ///         "count": 100,
    ///         "loggedInEmail": "charanraj.venkatesh@kumaran.com",
    ///         "viewType": "MyView",
    ///         "positionType": "UX Designer - Senior",
    ///         "type": "FTE",
    ///         "funding": "Base Funded",
    ///         "level": "4",
    ///         "timeline": {
    ///             "rangeType": "month",
    ///             "value": 3
    ///         }
    ///     }
    /// 
    /// Sample request (custom date range):
    /// 
    ///     POST /api/Approvals/List
    ///     {
    ///         "listTable": "Approval",
    ///         "approver": ["ramesh.kaliyaperumal@kumaran.com"],
    ///         "isPending": false,
    ///         "count": 500,
    ///         "timeline": {
    ///             "rangeType": "custom",
    ///             "startDate": "2026-01-01",
    ///             "endDate": "2026-03-31"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Approval list filter parameters</param>
    /// <response code="200">Success - Returns approval list data</response>
    /// <response code="400">Bad Request - Invalid input</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost("List")]
    [ProducesResponseType(typeof(ApprovalListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApprovalListResponse>> GetApprovalList([FromBody] ApprovalListRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting approval list for {ApproverCount} approvers, IsPending={IsPending}, Count={Count}, ViewType={ViewType}, Filters=[Position:{PositionType}, Type:{Type}, Funding:{Funding}, Level:{Level}, Timeline:{Timeline}]",
                request.Approver?.Count ?? 0,
                request.IsPending,
                request.Count,
                request.ViewType ?? "None",
                request.PositionType ?? "None",
                request.Type ?? "None",
                request.Funding ?? "None",
                request.Level ?? "None",
                request.Timeline?.RangeType ?? "None");

            var result = await _approvalListService.GetApprovalListAsync(request);

            _logger.LogInformation(
                "Successfully retrieved {ItemCount} approval items",
                result.Body.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for approval list");
            return BadRequest(new 
            { 
                StatusCode = 400,
                Message = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval list");

            return StatusCode(500, new
            {
                StatusCode = 500,
                Message = "Error retrieving approval list"
            });
        }
    }

    /// <summary>
    /// Get delegated approval list
    /// </summary>
    /// <remarks>
    /// Returns approvals delegated to the current user from other approvers.
    /// The delegation must be active (current date falls between DelegateFrom and DelegateTo).
    /// 
    /// Sample request (pending approvals):
    /// 
    ///     POST /api/Approvals/Delegation
    ///     {
    ///         "currentUserEmail": "shobana.ganesh@kumaran.com",
    ///         "isPending": true,
    ///         "pageNumber": 1,
    ///         "pageSize": 10
    ///     }
    /// 
    /// Sample request (completed approvals):
    /// 
    ///     POST /api/Approvals/Delegation
    ///     {
    ///         "currentUserEmail": "shobana.ganesh@kumaran.com",
    ///         "isPending": false,
    ///         "pageNumber": 1,
    ///         "pageSize": 10
    ///     }
    /// </remarks>
    /// <param name="request">Delegation request with user email, pending flag, and pagination</param>
    /// <returns>List of delegated approvals with pagination metadata</returns>
    [HttpPost("Delegation")]
    [ProducesResponseType(typeof(ApprovalDelegationListResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetDelegatedApprovalList([FromBody] ApprovalDelegationListRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting delegated approval list for user: {UserEmail}, isPending: {IsPending}, Page: {PageNumber}, Size: {PageSize}",
                request.CurrentUserEmail,
                request.IsPending,
                request.PageNumber,
                request.PageSize);

            var result = await _approvalDelegationListService.GetApprovalDelegationListAsync(request);

            _logger.LogInformation(
                "Successfully retrieved {ItemCount} delegated approval items (Page {PageNumber} of {TotalPages})",
                result.Body.Count,
                result.Pagination?.CurrentPage,
                result.Pagination?.TotalPages);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for delegated approval list");
            return BadRequest(new
            {
                StatusCode = 400,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delegated approval list");

            return StatusCode(500, new
            {
                StatusCode = 500,
                Message = "Error retrieving delegated approval list"
            });
        }
    }

    /// <summary>
    /// Process approver action (Approve/Reject/Return)
    /// </summary>
    /// <remarks>
    /// Processes an approver's action on a request. Supports three actions:
    /// - **Approved**: Approves the request. If this is the last approver, completes the request. Otherwise, moves to next approver.
    /// - **Rejected**: Rejects the request and marks it as complete. Sends notifications to all previously approved approvers and delegates.
    /// - **Returned to Requestor**: Returns the request to the requestor for corrections. Sends notifications to all previously approved approvers and delegates.
    /// 
    /// Sample request (Approved):
    /// 
    ///     POST /api/Approvals/Action
    ///     {
    ///         "requestID": "dbcee50b-c555-f111-a825-6045bde7afdc",
    ///         "requestorEmail": "shobana.ganesh@kumaran.com",
    ///         "approverName": "Ramesh Kaliyaperumal",
    ///         "approverEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "approverAction": "Approved",
    ///         "comments": "Looks good, approved",
    ///         "isTesting": false
    ///     }
    /// 
    /// Sample request (Rejected):
    /// 
    ///     POST /api/Approvals/Action
    ///     {
    ///         "requestID": "dbcee50b-c555-f111-a825-6045bde7afdc",
    ///         "requestorEmail": "shobana.ganesh@kumaran.com",
    ///         "approverName": "Ramesh Kaliyaperumal",
    ///         "approverEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "approverAction": "Rejected",
    ///         "comments": "data rejects",
    ///         "isTesting": false
    ///     }
    /// 
    /// Sample request (Returned):
    /// 
    ///     POST /api/Approvals/Action
    ///     {
    ///         "requestID": "dbcee50b-c555-f111-a825-6045bde7afdc",
    ///         "requestorEmail": "shobana.ganesh@kumaran.com",
    ///         "approverName": "Ramesh Kaliyaperumal",
    ///         "approverEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "approverAction": "Returned to Requestor",
    ///         "comments": "Please provide more details",
    ///         "isTesting": false
    ///     }
    /// </remarks>
    /// <param name="request">Approver action request</param>
    /// <returns>Action result with message</returns>
    [HttpPost("Action")]
    [ProducesResponseType(typeof(ApproverActionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ProcessApproverAction([FromBody] ApproverActionRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Processing approver action: RequestID={RequestID}, Action={Action}, Approver={ApproverEmail}",
                request.RequestID,
                request.ApproverAction,
                request.ApproverEmail);

            var result = await _approverActionService.ProcessApproverActionAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200")
            {
                _logger.LogWarning(
                    "Approver action failed: {Message}",
                    result.Body.Message);

                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully processed approver action: {ResultStatus}",
                result.Body.ResultStatus);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for approver action");
            return BadRequest(new ApproverActionResponse
            {
                StatusCode = "400",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = new ApproverActionResponseBody
                {
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approver action");

            return StatusCode(500, new ApproverActionResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = new ApproverActionResponseBody
                {
                    Message = "Error processing approver action"
                }
            });
        }
    }
}
