namespace HART.Application.DTOs;

/// <summary>
/// Request for job family distribution statistics
/// </summary>
public class JobFamilyDistributionRequest
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
/// Response for job family distribution
/// </summary>
public class JobFamilyDistributionResponse
{
    public string StatusCode { get; set; } = "200";
    public JobFamilyDistributionBody Body { get; set; } = new();
}

/// <summary>
/// Body of job family distribution response
/// </summary>
public class JobFamilyDistributionBody
{
    public string Status { get; set; } = "success";
    public List<JobFamilyCount> Data { get; set; } = new();
}

/// <summary>
/// Job family count item
/// </summary>
public class JobFamilyCount
{
    public string JobFamily { get; set; } = string.Empty;
    public int Count { get; set; }
}
