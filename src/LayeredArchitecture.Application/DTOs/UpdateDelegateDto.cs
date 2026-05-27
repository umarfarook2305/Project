namespace HART.Application.DTOs;

/// <summary>
/// Request to update a delegation
/// </summary>
public class UpdateDelegateRequest
{
    public string DelegateId { get; set; } = string.Empty;
    public string? DelegateUser { get; set; }
    public string? DelegateUserEmail { get; set; }
    public bool? DelegationStatus { get; set; }
    public string? DelegateFrom { get; set; }
    public string? DelegateTo { get; set; }
    public string? BehalfUser { get; set; }
    public string? BehalfUserEmail { get; set; }
    public string? DelegatedBy { get; set; }
    public string? DelegatedByEmail { get; set; }
    public string? DelegatedOn { get; set; }
}

/// <summary>
/// Response for update delegate operation
/// </summary>
public class UpdateDelegateResponse
{
    public string StatusCode { get; set; } = "200";
    public UpdateDelegateBody Body { get; set; } = new();
}

/// <summary>
/// Body of update delegate response
/// </summary>
public class UpdateDelegateBody
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public string DelegateId { get; set; } = string.Empty;
}
