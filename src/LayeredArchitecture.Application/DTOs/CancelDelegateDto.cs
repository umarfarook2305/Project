namespace HART.Application.DTOs;

/// <summary>
/// Request to cancel an approval delegation
/// </summary>
public class CancelDelegateRequest
{
    public string DelegateID { get; set; } = string.Empty;  // GUID of delegate record
    public string CancelledOn { get; set; } = string.Empty; // Date when cancelled (yyyy-MM-dd)
}

/// <summary>
/// Response for cancel delegate operation
/// </summary>
public class CancelDelegateResponse
{
    public string StatusCode { get; set; } = "200";
    public Dictionary<string, string> Headers { get; set; } = new() { { "content-type", "application/json" } };
    public CancelDelegateBody Body { get; set; } = new();
}

/// <summary>
/// Body of cancel delegate response
/// </summary>
public class CancelDelegateBody
{
    public string Status { get; set; } = "200";
    public string Message { get; set; } = string.Empty;
    public CancelDelegateData? Data { get; set; }
}

/// <summary>
/// Cancel delegate result data
/// </summary>
public class CancelDelegateData
{
    public string DelegatedBy { get; set; } = string.Empty;     // Email of person who created delegation
    public string CancelledOn { get; set; } = string.Empty;     // Date when cancelled (ISO 8601 format)
}
