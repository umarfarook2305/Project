namespace HART.Application.DTOs;

/// <summary>
/// Request for position distribution status
/// </summary>
public class PositionDistributionStatusRequest
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
/// Response for position distribution status
/// </summary>
public class PositionDistributionStatusResponse
{
    public string StatusCode { get; set; } = "200";
    public PositionDistributionStatusBody Body { get; set; } = new();
}

/// <summary>
/// Body of position distribution status response
/// </summary>
public class PositionDistributionStatusBody
{
    public string Status { get; set; } = "success";
    public List<PositionDistributionItem> Data { get; set; } = new();
}

/// <summary>
/// Position distribution item (approved requests over time)
/// </summary>
public class PositionDistributionItem
{
    public string Period { get; set; } = string.Empty;      // "May 19" or "Apr 2024"
    public string Type { get; set; } = string.Empty;        // Vacancy type: "New", "Replacement", "CW Conversion"
    public int Count { get; set; }
    public string RequestTpe { get; set; } = string.Empty;  // "FTE" or "CW" (Note: typo matches sample response)
}
