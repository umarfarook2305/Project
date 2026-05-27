using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HART.API.Controllers;

/// <summary>
/// API Controller for Dashboard Statistics operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IStatusCardDetailsService _statusCardDetailsService;
    private readonly IFundingDistributionService _fundingDistributionService;
    private readonly IJobLevelApprovedService _jobLevelApprovedService;
    private readonly IApprovedRoleDistributionService _approvedRoleDistributionService;
    private readonly IJobFamilyDistributionService _jobFamilyDistributionService;
    private readonly IPendingStatusDistributionService _pendingStatusDistributionService;
    private readonly IPositionDistributionStatusService _positionDistributionStatusService;
    private readonly IHiringTrendDataService _hiringTrendDataService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IStatusCardDetailsService statusCardDetailsService,
        IFundingDistributionService fundingDistributionService,
        IJobLevelApprovedService jobLevelApprovedService,
        IApprovedRoleDistributionService approvedRoleDistributionService,
        IJobFamilyDistributionService jobFamilyDistributionService,
        IPendingStatusDistributionService pendingStatusDistributionService,
        IPositionDistributionStatusService positionDistributionStatusService,
        IHiringTrendDataService hiringTrendDataService,
        ILogger<DashboardController> logger)
    {
        _statusCardDetailsService = statusCardDetailsService;
        _fundingDistributionService = fundingDistributionService;
        _jobLevelApprovedService = jobLevelApprovedService;
        _approvedRoleDistributionService = approvedRoleDistributionService;
        _jobFamilyDistributionService = jobFamilyDistributionService;
        _pendingStatusDistributionService = pendingStatusDistributionService;
        _positionDistributionStatusService = positionDistributionStatusService;
        _hiringTrendDataService = hiringTrendDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get status card details (dashboard statistics)
    /// </summary>
    /// <remarks>
    /// Returns aggregated counts for dashboard cards:
    /// - Total requests
    /// - FTE vs CW breakdown
    /// - Status breakdown (Approved, Pending, Rejected)
    /// 
    /// **Filter Types:**
    /// - **DURATION**: Use duration parameter ('1M', '3M', '6M', '1Y', 'FY')
    /// - **FINANCIAL_YEAR**: Automatic financial year calculation (April 1 - March 31)
    /// - **CUSTOM**: Use startDate and endDate parameters
    /// 
    /// **View Types:**
    /// - **MyView**: Only requests created by logged-in user
    /// - **MyTeam**: Requests created by logged-in user and direct reports
    /// - **MyOrg**: Requests created by logged-in user and entire organization hierarchy
    /// - **All**: All requests (no email filter)
    /// 
    /// Sample request (1 month duration):
    /// 
    ///     POST /api/Dashboard/StatusCards
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyView"
    ///     }
    /// 
    /// Sample request (financial year):
    /// 
    ///     POST /api/Dashboard/StatusCards
    ///     {
    ///         "duration": "",
    ///         "filterType": "FINANCIAL_YEAR",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyTeam"
    ///     }
    /// 
    /// Sample request (custom date range):
    /// 
    ///     POST /api/Dashboard/StatusCards
    ///     {
    ///         "duration": "",
    ///         "filterType": "CUSTOM",
    ///         "startDate": "2026-01-01",
    ///         "endDate": "2026-03-31",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyOrg"
    ///     }
    /// </remarks>
    /// <param name="request">Status card details request</param>
    /// <returns>Dashboard statistics</returns>
    [HttpPost("StatusCards")]
    [ProducesResponseType(typeof(StatusCardDetailsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStatusCardDetails([FromBody] StatusCardDetailsRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting status card details: Email={LoggedInEmail}, FilterType={FilterType}, Duration={Duration}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.Duration,
                request.ViewType);

            var result = await _statusCardDetailsService.GetStatusCardDetailsAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Status card details request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved status card details: Total={Total}, FTE={FTE}, CW={CW}, Approved={Approved}, Pending={Pending}, Rejected={Rejected}",
                result.Body.Data.Total,
                result.Body.Data.FTE,
                result.Body.Data.CW,
                result.Body.Data.Approved,
                result.Body.Data.Pending,
                result.Body.Data.Rejected);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for status card details");
            return BadRequest(new StatusCardDetailsResponse
            {
                StatusCode = "400",
                Body = new StatusCardBody
                {
                    Status = "error",
                    Data = new StatusCardData()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status card details");

            return StatusCode(500, new StatusCardDetailsResponse
            {
                StatusCode = "500",
                Body = new StatusCardBody
                {
                    Status = "error",
                    Data = new StatusCardData()
                }
            });
        }
    }

    /// <summary>
    /// Get funding distribution statistics
    /// </summary>
    /// <remarks>
    /// Returns funding distribution breakdown:
    /// - Total requests
    /// - Project Funded (total, FTE, CW)
    /// - Base Funded (total, FTE, CW)
    /// 
    /// **Insights Types:**
    /// - **All**: All request types (FTE + CW)
    /// - **FTE**: Only FTE requests
    /// - **CW**: Only CW requests
    /// 
    /// Sample request (1 month, all insights):
    /// 
    ///     POST /api/Dashboard/FundingDistribution
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "insightsType": "All",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyView"
    ///     }
    /// 
    /// Sample request (FTE only):
    /// 
    ///     POST /api/Dashboard/FundingDistribution
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "insightsType": "FTE",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyView"
    ///     }
    /// </remarks>
    /// <param name="request">Funding distribution request</param>
    /// <returns>Funding distribution statistics</returns>
    [HttpPost("FundingDistribution")]
    [ProducesResponseType(typeof(FundingDistributionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetFundingDistribution([FromBody] FundingDistributionRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting funding distribution: Email={LoggedInEmail}, FilterType={FilterType}, InsightsType={InsightsType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.InsightsType,
                request.ViewType);

            var result = await _fundingDistributionService.GetFundingDistributionAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Funding distribution request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved funding distribution: Total={Total}, ProjectFunded={ProjectFunded}, BaseFunded={BaseFunded}",
                result.Body.Data.Total,
                result.Body.Data.ProjectFunded.Total,
                result.Body.Data.BaseFunded.Total);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for funding distribution");
            return BadRequest(new FundingDistributionResponse
            {
                StatusCode = "400",
                Body = new FundingDistributionBody
                {
                    Status = "error",
                    Data = new FundingDistributionData()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving funding distribution");

            return StatusCode(500, new FundingDistributionResponse
            {
                StatusCode = "500",
                Body = new FundingDistributionBody
                {
                    Status = "error",
                    Data = new FundingDistributionData()
                }
            });
        }
    }

    /// <summary>
    /// Get approved positions grouped by job level
    /// </summary>
    /// <remarks>
    /// Returns approved position counts grouped by job level:
    /// 
    /// Sample request (1 month, all insights):
    /// 
    ///     POST /api/Dashboard/JobLevelApproved
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "insightsType": "All",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyView"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "data": {
    ///                 "body": [
    ///                     { "level": "4", "count": 1 },
    ///                     { "level": "5", "count": 2 }
    ///                 ]
    ///             }
    ///         }
    ///     }
    /// </remarks>
    /// <param name="request">Job level approved request</param>
    /// <returns>Approved positions grouped by job level</returns>
    [HttpPost("JobLevelApproved")]
    [ProducesResponseType(typeof(JobLevelApprovedResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetJobLevelApproved([FromBody] JobLevelApprovedRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting job level approved: Email={LoggedInEmail}, FilterType={FilterType}, InsightsType={InsightsType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.InsightsType,
                request.ViewType);

            var result = await _jobLevelApprovedService.GetJobLevelApprovedAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200")
            {
                _logger.LogWarning("Job level approved request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved job level approved: Count={Count}",
                result.Body.Data.Body.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for job level approved");
            return BadRequest(new JobLevelApprovedResponse
            {
                StatusCode = "400",
                Body = new JobLevelApprovedBody
                {
                    Data = new JobLevelApprovedData
                    {
                        Body = new List<JobLevelCount>()
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job level approved");

            return StatusCode(500, new JobLevelApprovedResponse
            {
                StatusCode = "500",
                Body = new JobLevelApprovedBody
                {
                    Data = new JobLevelApprovedData
                    {
                        Body = new List<JobLevelCount>()
                    }
                }
            });
        }
    }

    /// <summary>
    /// Get approved FTE positions grouped by role type
    /// </summary>
    /// <remarks>
    /// Returns approved FTE position counts grouped by role type (e.g., Individual Contributor, Manager):
    /// 
    /// Sample request (1 month, My Team view):
    /// 
    ///     POST /api/Dashboard/ApprovedRoleDistribution
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "loggedInEmail": "prabakar.krishnamoorthy@kumaran.com",
    ///         "viewType": "My Team"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "totalCount": 1,
    ///             "data": [
    ///                 { "roleType": "Individual Contributor", "count": 1 }
    ///             ]
    ///         }
    ///     }
    /// 
    /// Note: This endpoint only returns FTE positions (not CW).
    /// </remarks>
    /// <param name="request">Approved role distribution request</param>
    /// <returns>Approved FTE positions grouped by role type</returns>
    [HttpPost("ApprovedRoleDistribution")]
    [ProducesResponseType(typeof(ApprovedRoleDistributionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetApprovedRoleDistribution([FromBody] ApprovedRoleDistributionRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting approved role distribution: Email={LoggedInEmail}, FilterType={FilterType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.ViewType);

            var result = await _approvedRoleDistributionService.GetApprovedRoleDistributionAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Approved role distribution request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved approved role distribution: TotalCount={TotalCount}, RoleTypes={RoleTypes}",
                result.Body.TotalCount,
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for approved role distribution");
            return BadRequest(new ApprovedRoleDistributionResponse
            {
                StatusCode = "400",
                Body = new ApprovedRoleDistributionBody
                {
                    Status = "error",
                    TotalCount = 0,
                    Data = new List<RoleTypeCount>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approved role distribution");

            return StatusCode(500, new ApprovedRoleDistributionResponse
            {
                StatusCode = "500",
                Body = new ApprovedRoleDistributionBody
                {
                    Status = "error",
                    TotalCount = 0,
                    Data = new List<RoleTypeCount>()
                }
            });
        }
    }

    /// <summary>
    /// Get approved positions grouped by job family
    /// </summary>
    /// <remarks>
    /// Returns approved position counts grouped by job family (e.g., AI Governance, AI Scientist):
    /// 
    /// Sample request (1 month, all insights):
    /// 
    ///     POST /api/Dashboard/JobFamilyDistribution
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "startDate": "",
    ///         "endDate": "",
    ///         "insightsType": "All",
    ///         "loggedInEmail": "prabakar.krishnamoorthy@kumaran.com",
    ///         "viewType": "MyView"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "data": [
    ///                 { "jobFamily": "AI Governance", "count": 2 },
    ///                 { "jobFamily": "AI Scientist", "count": 1 }
    ///             ]
    ///         }
    ///     }
    /// </remarks>
    /// <param name="request">Job family distribution request</param>
    /// <returns>Approved positions grouped by job family</returns>
    [HttpPost("JobFamilyDistribution")]
    [ProducesResponseType(typeof(JobFamilyDistributionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetJobFamilyDistribution([FromBody] JobFamilyDistributionRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting job family distribution: Email={LoggedInEmail}, FilterType={FilterType}, InsightsType={InsightsType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.InsightsType,
                request.ViewType);

            var result = await _jobFamilyDistributionService.GetJobFamilyDistributionAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Job family distribution request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved job family distribution: Count={Count}",
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for job family distribution");
            return BadRequest(new JobFamilyDistributionResponse
            {
                StatusCode = "400",
                Body = new JobFamilyDistributionBody
                {
                    Status = "error",
                    Data = new List<JobFamilyCount>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job family distribution");

            return StatusCode(500, new JobFamilyDistributionResponse
            {
                StatusCode = "500",
                Body = new JobFamilyDistributionBody
                {
                    Status = "error",
                    Data = new List<JobFamilyCount>()
                }
            });
        }
    }

    /// <summary>
    /// Get pending approvals grouped by requester role
    /// </summary>
    /// <remarks>
    /// Returns pending approval counts grouped by requester role:
    /// 
    /// Sample request (1 month, MyOrg view):
    /// 
    ///     POST /api/Dashboard/PendingStatus
    ///     {
    ///         "duration": "1M",
    ///         "insightsType": "All",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyOrg"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "data": [
    ///                 { "requesterRole": "Director", "count": 1 },
    ///                 { "requesterRole": "HR Business Partner", "count": 2 },
    ///                 { "requesterRole": "Vice President", "count": 2 }
    ///             ]
    ///         }
    ///     }
    /// 
    /// Note: Supports financialYear object in request for compatibility.
    /// </remarks>
    /// <param name="request">Pending status distribution request</param>
    /// <returns>Pending approvals grouped by requester role</returns>
    [HttpPost("PendingStatus")]
    [ProducesResponseType(typeof(PendingStatusDistributionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetPendingStatusDistribution([FromBody] PendingStatusDistributionRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting pending status distribution: Email={LoggedInEmail}, FilterType={FilterType}, InsightsType={InsightsType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.InsightsType,
                request.ViewType);

            var result = await _pendingStatusDistributionService.GetPendingStatusDistributionAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Pending status distribution request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved pending status distribution: Count={Count}",
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for pending status distribution");
            return BadRequest(new PendingStatusDistributionResponse
            {
                StatusCode = "400",
                Body = new PendingStatusDistributionBody
                {
                    Status = "error",
                    Data = new List<RequesterRoleCount>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending status distribution");

            return StatusCode(500, new PendingStatusDistributionResponse
            {
                StatusCode = "500",
                Body = new PendingStatusDistributionBody
                {
                    Status = "error",
                    Data = new List<RequesterRoleCount>()
                }
            });
        }
    }

    /// <summary>
    /// Get position distribution status over time (approved requests by vacancy type)
    /// </summary>
    /// <remarks>
    /// Returns approved requests distributed over time periods grouped by vacancy type:
    /// 
    /// Sample request (1 month, MyOrg view):
    /// 
    ///     POST /api/Dashboard/PositionDistributionStatus
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "insightsType": "All",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyOrg"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "data": [
    ///                 { "period": "May 19", "type": "CW Conversion", "count": 1, "requestTpe": "FTE" },
    ///                 { "period": "Apr 29", "type": "New", "count": 1, "requestTpe": "FTE" },
    ///                 { "period": "Apr 29", "type": "Replacement", "count": 1, "requestTpe": "FTE" }
    ///             ]
    ///         }
    ///     }
    /// 
    /// Note: 
    /// - For 1M duration, groups by day (e.g., "May 19")
    /// - For other durations, groups by month (e.g., "Apr 2024")
    /// - Only includes approved requests
    /// </remarks>
    /// <param name="request">Position distribution status request</param>
    /// <returns>Approved requests distributed over time by vacancy type</returns>
    [HttpPost("PositionDistributionStatus")]
    [ProducesResponseType(typeof(PositionDistributionStatusResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetPositionDistributionStatus([FromBody] PositionDistributionStatusRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting position distribution status: Email={LoggedInEmail}, FilterType={FilterType}, Duration={Duration}, InsightsType={InsightsType}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.Duration,
                request.InsightsType,
                request.ViewType);

            var result = await _positionDistributionStatusService.GetPositionDistributionStatusAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Position distribution status request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved position distribution status: Count={Count}",
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for position distribution status");
            return BadRequest(new PositionDistributionStatusResponse
            {
                StatusCode = "400",
                Body = new PositionDistributionStatusBody
                {
                    Status = "error",
                    Data = new List<PositionDistributionItem>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving position distribution status");

            return StatusCode(500, new PositionDistributionStatusResponse
            {
                StatusCode = "500",
                Body = new PositionDistributionStatusBody
                {
                    Status = "error",
                    Data = new List<PositionDistributionItem>()
                }
            });
        }
    }

    /// <summary>
    /// Get hiring trend data (FTE and CW counts over 6 time buckets)
    /// </summary>
    /// <remarks>
    /// Returns FTE and CW request counts distributed across 6 time buckets:
    /// 
    /// Sample request (1 month, MyOrg view):
    /// 
    ///     POST /api/Dashboard/HiringTrendData
    ///     {
    ///         "duration": "1M",
    ///         "filterType": "DURATION",
    ///         "loggedInEmail": "ramesh.kaliyaperumal@kumaran.com",
    ///         "viewType": "MyOrg"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "statusCode": "200",
    ///         "body": {
    ///             "status": "success",
    ///             "data": [
    ///                 { "date": "Apr 26", "FTE": "2", "CW": "0" },
    ///                 { "date": "May 01", "FTE": "1", "CW": "0" },
    ///                 { "date": "May 06", "FTE": "0", "CW": "0" },
    ///                 { "date": "May 11", "FTE": "0", "CW": "0" },
    ///                 { "date": "May 16", "FTE": "1", "CW": "0" },
    ///                 { "date": "May 21", "FTE": "0", "CW": "0" }
    ///             ]
    ///         }
    ///     }
    /// 
    /// Note: 
    /// - Always returns exactly 6 data points (time buckets)
    /// - For 1M duration, dates formatted as "MMM dd" (e.g., "Apr 26")
    /// - For other durations, dates formatted as "MMM yyyy" (e.g., "Apr 2024")
    /// - Counts returned as strings
    /// </remarks>
    /// <param name="request">Hiring trend data request</param>
    /// <returns>FTE and CW counts across 6 time buckets</returns>
    [HttpPost("HiringTrendData")]
    [ProducesResponseType(typeof(HiringTrendDataResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetHiringTrendData([FromBody] HiringTrendDataRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Getting hiring trend data: Email={LoggedInEmail}, FilterType={FilterType}, Duration={Duration}, ViewType={ViewType}",
                request.LoggedInEmail,
                request.FilterType,
                request.Duration,
                request.ViewType);

            var result = await _hiringTrendDataService.GetHiringTrendDataAsync(request);

            // Check if response indicates error
            if (result.StatusCode != "200" || result.Body.Status != "success")
            {
                _logger.LogWarning("Hiring trend data request failed");
                return StatusCode(int.Parse(result.StatusCode), result);
            }

            _logger.LogInformation(
                "Successfully retrieved hiring trend data: DataPoints={Count}",
                result.Body.Data.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data for hiring trend data");
            return BadRequest(new HiringTrendDataResponse
            {
                StatusCode = "400",
                Body = new HiringTrendDataBody
                {
                    Status = "error",
                    Data = new List<HiringTrendDataPoint>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hiring trend data");

            return StatusCode(500, new HiringTrendDataResponse
            {
                StatusCode = "500",
                Body = new HiringTrendDataBody
                {
                    Status = "error",
                    Data = new List<HiringTrendDataPoint>()
                }
            });
        }
    }
}
