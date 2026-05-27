namespace HART.Application.DTOs;

/// <summary>
/// Request for job level approved statistics
/// </summary>
public class JobLevelApprovedRequest
{
    public string? Duration { get; set; }           // '1M', '3M', '6M', '1Y', 'FY'
    public string FilterType { get; set; } = "DURATION"; // 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
    public string? StartDate { get; set; }          // Custom start date (yyyy-MM-dd)
    public string? EndDate { get; set; }            // Custom end date (yyyy-MM-dd)
    public string InsightsType { get; set; } = "All"; // 'All', 'FTE', 'CW'
    public string LoggedInEmail { get; set; } = string.Empty;
    public string ViewType { get; set; } = "MyView"; // 'MyView', 'MyTeam', 'MyOrg', 'All'
}

/// <summary>
/// Response for job level approved
/// </summary>
public class JobLevelApprovedResponse
{
    public string StatusCode { get; set; } = "200";
    public JobLevelApprovedBody Body { get; set; } = new();
}

/// <summary>
/// Body of job level approved response
/// </summary>
public class JobLevelApprovedBody
{
    public JobLevelApprovedData Data { get; set; } = new();
}

/// <summary>
/// Job level approved data
/// </summary>
public class JobLevelApprovedData
{
    public List<JobLevelCount> Body { get; set; } = new();
}

/// <summary>
/// Job level count item
/// </summary>
public class JobLevelCount
{
    public string Level { get; set; } = string.Empty;
    public int Count { get; set; }
}
