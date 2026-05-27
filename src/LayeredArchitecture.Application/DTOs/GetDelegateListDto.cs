namespace HART.Application.DTOs;

/// <summary>
/// Request to get delegate list
/// </summary>
public class GetDelegateListRequest
{
    public string EmailId { get; set; } = string.Empty;     // Behalf user email
    public bool DelegationStatus { get; set; } = true;      // true = active, false = inactive/expired
}

/// <summary>
/// Response for get delegate list
/// </summary>
public class GetDelegateListResponse
{
    public string StatusCode { get; set; } = "200";
    public GetDelegateListBody Body { get; set; } = new();
}

/// <summary>
/// Body of get delegate list response
/// </summary>
public class GetDelegateListBody
{
    public string Status { get; set; } = "success";
    public List<DelegateItem> Data { get; set; } = new();
}

/// <summary>
/// Delegate item
/// </summary>
public class DelegateItem
{
    public string DelegateId { get; set; } = string.Empty;          // Delegate code (e.g., "0023")
    public string DelegateUser { get; set; } = string.Empty;        // Delegate user name
    public string DelegateUserEmail { get; set; } = string.Empty;   // Delegate user email
    public string DelegationStatus { get; set; } = "True";          // "True" or "False" as string
    public string DelegateFrom { get; set; } = string.Empty;        // Start date (ISO 8601)
    public string DelegateTo { get; set; } = string.Empty;          // End date (ISO 8601)
    public string BehalfUser { get; set; } = string.Empty;          // Behalf user name
    public string BehalfUserEmail { get; set; } = string.Empty;     // Behalf user email
    public string DelegatedBy { get; set; } = string.Empty;         // Delegated by name
    public string DelegatedByEmail { get; set; } = string.Empty;    // Delegated by email
    public string DelegatedOn { get; set; } = string.Empty;         // Delegated on date (ISO 8601)
    public string UniqueID { get; set; } = string.Empty;            // Delegate GUID
}
