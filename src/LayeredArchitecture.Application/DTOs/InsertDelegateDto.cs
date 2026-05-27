namespace HART.Application.DTOs;

/// <summary>
/// Request to create a new delegation
/// </summary>
public class InsertDelegateRequest
{
    public string BehalfUserEmail { get; set; } = string.Empty;
    public string DelegatedByEmail { get; set; } = string.Empty;
    public string DelegateUserEmail { get; set; } = string.Empty;
    public bool DelegationStatus { get; set; }
    public string? BehalfUser { get; set; }
    public string? DelegatedBy { get; set; }
    public string? DelegatedOn { get; set; }
    public string DelegateFrom { get; set; } = string.Empty;
    public string DelegateTo { get; set; } = string.Empty;
    public string? DelegateUser { get; set; }
}

/// <summary>
/// Response for insert delegate operation
/// </summary>
public class InsertDelegateResponse
{
    public string StatusCode { get; set; } = "200";
    public InsertDelegateBody Body { get; set; } = new();
}

/// <summary>
/// Body of insert delegate response
/// </summary>
public class InsertDelegateBody
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public string? DelegateId { get; set; }
    public string? DelegateCode { get; set; }
}
