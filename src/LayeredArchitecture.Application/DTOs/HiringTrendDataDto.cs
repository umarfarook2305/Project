namespace HART.Application.DTOs;

/// <summary>
/// Request for hiring trend data
/// </summary>
public class HiringTrendDataRequest
{
    public string? Duration { get; set; }           // '1M', '3M', '6M', '1Y', 'FY'
    public string FilterType { get; set; } = "DURATION"; // 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
    public string? StartDate { get; set; }          // Custom start date (yyyy-MM-dd)
    public string? EndDate { get; set; }            // Custom end date (yyyy-MM-dd)
    public string LoggedInEmail { get; set; } = string.Empty;
    public string ViewType { get; set; } = "MyView"; // 'MyView', 'MyTeam', 'MyOrg', 'All'
}

/// <summary>
/// Response for hiring trend data
/// </summary>
public class HiringTrendDataResponse
{
    public string StatusCode { get; set; } = "200";
    public HiringTrendDataBody Body { get; set; } = new();
}

/// <summary>
/// Body of hiring trend data response
/// </summary>
public class HiringTrendDataBody
{
    public string Status { get; set; } = "success";
    public List<HiringTrendDataPoint> Data { get; set; } = new();
}

/// <summary>
/// Hiring trend data point (FTE and CW counts for a time bucket)
/// </summary>
public class HiringTrendDataPoint
{
    public string Date { get; set; } = string.Empty;  // "Apr 26" or "May 2024"
    public string FTE { get; set; } = "0";            // FTE count as string
    public string CW { get; set; } = "0";             // CW count as string
}
