namespace HART.Application.DTOs;

/// <summary>
/// Request for approved role distribution statistics
/// </summary>
public class ApprovedRoleDistributionRequest
{
    public string? Duration { get; set; }           // '1M', '3M', '6M', '1Y', 'FY'
    public string FilterType { get; set; } = "DURATION"; // 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
    public string? StartDate { get; set; }          // Custom start date (yyyy-MM-dd)
    public string? EndDate { get; set; }            // Custom end date (yyyy-MM-dd)
    public string LoggedInEmail { get; set; } = string.Empty;
    public string ViewType { get; set; } = "MyView"; // 'MyView', 'MyTeam', 'My Team', 'MyOrg', 'All'
}

/// <summary>
/// Response for approved role distribution
/// </summary>
public class ApprovedRoleDistributionResponse
{
    public string StatusCode { get; set; } = "200";
    public ApprovedRoleDistributionBody Body { get; set; } = new();
}

/// <summary>
/// Body of approved role distribution response
/// </summary>
public class ApprovedRoleDistributionBody
{
    public string Status { get; set; } = "success";
    public int TotalCount { get; set; }
    public List<RoleTypeCount> Data { get; set; } = new();
}

/// <summary>
/// Role type count item
/// </summary>
public class RoleTypeCount
{
    public string RoleType { get; set; } = string.Empty;
    public int Count { get; set; }
}
