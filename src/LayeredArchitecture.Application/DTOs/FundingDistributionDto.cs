namespace HART.Application.DTOs;

/// <summary>
/// Request for funding distribution statistics
/// </summary>
public class FundingDistributionRequest
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
/// Response for funding distribution
/// </summary>
public class FundingDistributionResponse
{
    public string StatusCode { get; set; } = "200";
    public FundingDistributionBody Body { get; set; } = new();
}

/// <summary>
/// Body of funding distribution response
/// </summary>
public class FundingDistributionBody
{
    public string Status { get; set; } = "success";
    public FundingDistributionData Data { get; set; } = new();
}

/// <summary>
/// Funding distribution data
/// </summary>
public class FundingDistributionData
{
    public string Total { get; set; } = "0";
    public FundingTypeDetail ProjectFunded { get; set; } = new();
    public FundingTypeDetail BaseFunded { get; set; } = new();
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

/// <summary>
/// Funding type detail (Project Funded or Base Funded)
/// </summary>
public class FundingTypeDetail
{
    public string Total { get; set; } = "0";
    public string FTE { get; set; } = "0";
    public string CW { get; set; } = "0";
}
