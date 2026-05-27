namespace HART.Application.DTOs;

/// <summary>
/// Request for status card details (dashboard statistics)
/// </summary>
public class StatusCardDetailsRequest
{
    public string? Duration { get; set; }           // '1M', '3M', '6M', '1Y', 'FY'
    public string FilterType { get; set; } = "DURATION"; // 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
    public string? StartDate { get; set; }          // Custom start date (yyyy-MM-dd)
    public string? EndDate { get; set; }            // Custom end date (yyyy-MM-dd)
    public string LoggedInEmail { get; set; } = string.Empty;
    public string ViewType { get; set; } = "MyView"; // 'MyView', 'MyTeam', 'MyOrg', 'All'
}

/// <summary>
/// Response for status card details
/// </summary>
public class StatusCardDetailsResponse
{
    public string StatusCode { get; set; } = "200";
    public StatusCardBody Body { get; set; } = new();
}

/// <summary>
/// Body of status card response
/// </summary>
public class StatusCardBody
{
    public string Status { get; set; } = "success";
    public StatusCardData Data { get; set; } = new();
}

/// <summary>
/// Statistical data for status cards
/// </summary>
public class StatusCardData
{
    public int Total { get; set; }
    public int FTE { get; set; }
    public int CW { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
