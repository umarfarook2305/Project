namespace HART.Application.DTOs;

/// <summary>
/// Request for pending status distribution statistics
/// </summary>
public class PendingStatusDistributionRequest
{
    public string? Duration { get; set; }           // '1M', '3M', '6M', '1Y', 'FY'
    public string FilterType { get; set; } = "DURATION"; // 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
    public string? StartDate { get; set; }          // Custom start date (yyyy-MM-dd)
    public string? EndDate { get; set; }            // Custom end date (yyyy-MM-dd)
    public string InsightsType { get; set; } = "All"; // 'All', 'FTE', 'CW'
    public string LoggedInEmail { get; set; } = string.Empty;
    public string ViewType { get; set; } = "MyView"; // 'MyView', 'MyTeam', 'MyOrg', 'All'
    public FinancialYearInfo? FinancialYear { get; set; }
}

/// <summary>
/// Financial year information (for compatibility)
/// </summary>
public class FinancialYearInfo
{
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<string>? EmailList { get; set; }
    public string? InsightsType { get; set; }
}

/// <summary>
/// Response for pending status distribution
/// </summary>
public class PendingStatusDistributionResponse
{
    public string StatusCode { get; set; } = "200";
    public PendingStatusDistributionBody Body { get; set; } = new();
}

/// <summary>
/// Body of pending status distribution response
/// </summary>
public class PendingStatusDistributionBody
{
    public string Status { get; set; } = "success";
    public List<RequesterRoleCount> Data { get; set; } = new();
}

/// <summary>
/// Requester role count item
/// </summary>
public class RequesterRoleCount
{
    public string RequesterRole { get; set; } = string.Empty;
    public int Count { get; set; }
}
