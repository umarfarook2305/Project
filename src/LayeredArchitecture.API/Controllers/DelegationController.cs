using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Approval Delegation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DelegationController : ControllerBase
{
    private readonly ICancelDelegateService _cancelDelegateService;
    private readonly IGetDelegateListService _getDelegateListService;
    private readonly IUpdateDelegateService _updateDelegateService;
    private readonly IInsertDelegateService _insertDelegateService;
    private readonly ILogger<DelegationController> _logger;

    public DelegationController(
        ICancelDelegateService cancelDelegateService,
        IGetDelegateListService getDelegateListService,
        IUpdateDelegateService updateDelegateService,
        IInsertDelegateService insertDelegateService,
        ILogger<DelegationController> logger)
    {
        _cancelDelegateService = cancelDelegateService;
        _getDelegateListService = getDelegateListService;
        _updateDelegateService = updateDelegateService;
        _insertDelegateService = insertDelegateService;
        _logger = logger;
    }

    /// <summary>
    /// Cancel an approval delegation
    /// </summary>
    /// <remarks>
    /// Cancels an existing approval delegation by setting its status to inactive:
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Delegation/CancelDelegate
    ///     {
    ///         "delegateID": "b5eec2e4-ec4d-f111-bec6-70a8a5695569",
    ///         "cancelledOn": "2026-05-12"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "headers": {
    ///             "content-type": "application/json"
    ///         },
    ///         "body": {
    ///             "status": "200",
    ///             "message": "Delegate Cancelled Successfully",
    ///             "data": {
    ///                 "delegatedBy": "paulnishanth.ramakrishnan@kumaran.com",
    ///                 "cancelledOn": "2026-05-12T00:00:00Z"
    ///             }
    ///         }
    ///     }
    /// 
    /// Note: Sets DelegationStatus to false (inactive) and records CancelledOn date
    /// </remarks>
    /// <param name="request">Cancel delegate request</param>
    /// <returns>Result of cancellation operation</returns>
    [HttpPost("CancelDelegate")]
    [ProducesResponseType(typeof(CancelDelegateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CancelDelegate([FromBody] CancelDelegateRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Cancelling delegate: DelegateID={DelegateID}, CancelledOn={CancelledOn}",
                request.DelegateID,
                request.CancelledOn);

            var result = await _cancelDelegateService.CancelDelegateAsync(request);

            // Check status code
            if (result.StatusCode == "404")
            {
                _logger.LogWarning("Delegate not found: DelegateID={DelegateID}", request.DelegateID);
                return NotFound(result);
            }

            if (result.StatusCode != "200")
            {
                _logger.LogWarning("Cancel delegate request failed: StatusCode={StatusCode}", result.StatusCode);
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully cancelled delegate: DelegateID={DelegateID}, DelegatedBy={DelegatedBy}",
                request.DelegateID,
                result.Body.Data?.DelegatedBy);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for cancel delegate");
            return BadRequest(new CancelDelegateResponse
            {
                StatusCode = "400",
                Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                Body = new CancelDelegateBody
                {
                    Status = "400",
                    Message = ex.Message,
                    Data = null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling delegate");

            return StatusCode(500, new CancelDelegateResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                Body = new CancelDelegateBody
                {
                    Status = "500",
                    Message = "Error while Cancelling Delegate",
                    Data = null
                }
            });
        }
    }

    /// <summary>
    /// Get list of delegations for a user
    /// </summary>
    /// <remarks>
    /// Returns list of delegations based on behalf user email and delegation status:
    /// 
    /// Sample request (active delegations):
    /// 
    ///     POST /api/Delegation/GetDelegateList
    ///     {
    ///         "emailId": "mohans-office@kumaran.com",
    ///         "delegationStatus": true
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "data": [
    ///                 {
    ///                     "delegateId": "0023",
    ///                     "delegateUser": "Shobana Ganesh",
    ///                     "delegateUserEmail": "shobana.ganesh@kumaran.com",
    ///                     "delegationStatus": "True",
    ///                     "delegateFrom": "2026-05-21T00:00:00Z",
    ///                     "delegateTo": "2026-05-31T00:00:00Z",
    ///                     "behalfUser": "Mohans-Office",
    ///                     "behalfUserEmail": "mohans-office@kumaran.com",
    ///                     "delegatedBy": "Mohans-Office",
    ///                     "delegatedByEmail": "mohans-office@kumaran.com",
    ///                     "delegatedOn": "2026-05-22T00:00:00Z",
    ///                     "uniqueID": "18695e4e-0155-f111-bec6-7ced8d26d4b6"
    ///                 }
    ///             ]
    ///         }
    ///     }
    /// 
    /// Note:
    /// - delegationStatus = true: Returns active delegations (delegateTo >= today)
    /// - delegationStatus = false: Returns inactive/expired delegations (delegateTo < today)
    /// </remarks>
    /// <param name="request">Get delegate list request</param>
    /// <returns>List of delegations</returns>
    [HttpPost("GetDelegateList")]
    [ProducesResponseType(typeof(GetDelegateListResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetDelegateList([FromBody] GetDelegateListRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting delegate list: EmailId={EmailId}, DelegationStatus={DelegationStatus}",
                request.EmailId,
                request.DelegationStatus);

            var result = await _getDelegateListService.GetDelegateListAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Get delegate list request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved delegate list: Count={Count}",
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for get delegate list");
            return BadRequest(new GetDelegateListResponse
            {
                StatusCode = "400",
                Body = new GetDelegateListBody
                {
                    Status = "error",
                    Data = new List<DelegateItem>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delegate list");

            return StatusCode(500, new GetDelegateListResponse
            {
                StatusCode = "500",
                Body = new GetDelegateListBody
                {
                    Status = "error",
                    Data = new List<DelegateItem>()
                }
            });
        }
    }

    /// <summary>
    /// Update an existing delegation
    /// </summary>
    [HttpPut("UpdateDelegate")]
    [ProducesResponseType(typeof(UpdateDelegateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateDelegate([FromBody] UpdateDelegateRequest request)
    {
        try
        {
            var result = await _updateDelegateService.UpdateDelegateAsync(request);

            if (result.StatusCode == "404")
                return NotFound(result);

            if (result.StatusCode != "200")
                return StatusCode(int.Parse(result.StatusCode), result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new UpdateDelegateResponse
            {
                StatusCode = "400",
                Body = new UpdateDelegateBody { Status = "error", Message = ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delegate");
            return StatusCode(500, new UpdateDelegateResponse
            {
                StatusCode = "500",
                Body = new UpdateDelegateBody { Status = "error", Message = "Failed to update delegation" }
            });
        }
    }

    /// <summary>
    /// Create a new delegation
    /// </summary>
    [HttpPost("InsertDelegate")]
    [ProducesResponseType(typeof(InsertDelegateResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> InsertDelegate([FromBody] InsertDelegateRequest request)
    {
        try
        {
            var result = await _insertDelegateService.InsertDelegateAsync(request);

            if (result.StatusCode != "200")
                return StatusCode(int.Parse(result.StatusCode), result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new InsertDelegateResponse
            {
                StatusCode = "400",
                Body = new InsertDelegateBody { Status = "error", Message = ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delegate");
            return StatusCode(500, new InsertDelegateResponse
            {
                StatusCode = "500",
                Body = new InsertDelegateBody { Status = "error", Message = "Failed to create delegation" }
            });
        }
    }
}
