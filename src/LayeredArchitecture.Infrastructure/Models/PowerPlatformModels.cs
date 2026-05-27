using System.Text.Json.Serialization;

namespace HART.Infrastructure.Models;

public class PowerPlatformResponse<T>
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; set; } = string.Empty;

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = new();

    [JsonPropertyName("body")]
    public T Body { get; set; } = default!;
}

public class LookupDataResponse
{
    [JsonPropertyName("LineOfBusiness")]
    public List<LookupItemResponse>? LineOfBusiness { get; set; }

    [JsonPropertyName("ProjectTheme")]
    public List<LookupItemResponse>? ProjectTheme { get; set; }

    [JsonPropertyName("SWPRole")]
    public List<LookupItemResponse> SWPRole { get; set; } = new();

    [JsonPropertyName("FundingType")]
    public List<LookupItemResponse> FundingType { get; set; } = new();

    [JsonPropertyName("CountryList")]
    public List<LookupItemResponse> CountryList { get; set; } = new();

    [JsonPropertyName("Status")]
    public List<LookupItemResponse> Status { get; set; } = new();

    [JsonPropertyName("Joblevel")]
    public List<LookupItemResponse>? Joblevel { get; set; }

    [JsonPropertyName("PositionType")]
    public List<LookupItemResponse>? PositionType { get; set; }

    [JsonPropertyName("RoleType")]
    public List<LookupItemResponse>? RoleType { get; set; }

    [JsonPropertyName("StatusOfPosition")]
    public List<LookupItemResponse>? StatusOfPosition { get; set; }
}

public class LookupItemResponse
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
