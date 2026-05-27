namespace HART.Application.DTOs;

public class LookupDataRequest
{
    public string Type { get; set; } = string.Empty;
}

public class LookupItemDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// CW Response
public class CWLookupDataDto
{
    public List<LookupItemDto> LineOfBusiness { get; set; } = new();
    public List<LookupItemDto> ProjectTheme { get; set; } = new();
    public List<LookupItemDto> SWPRole { get; set; } = new();
    public List<LookupItemDto> FundingType { get; set; } = new();
    public List<LookupItemDto> CountryList { get; set; } = new();
    public List<LookupItemDto> Status { get; set; } = new();
}

// FTE Response
public class FTELookupDataDto
{
    public List<LookupItemDto> Joblevel { get; set; } = new();
    public List<LookupItemDto> SWPRole { get; set; } = new();
    public List<LookupItemDto> PositionType { get; set; } = new();
    public List<LookupItemDto> FundingType { get; set; } = new();
    public List<LookupItemDto> RoleType { get; set; } = new();
    public List<LookupItemDto> CountryList { get; set; } = new();
    public List<LookupItemDto> Status { get; set; } = new();
    public List<LookupItemDto> StatusOfPosition { get; set; } = new();
}
