namespace HART.Application.DTOs;

/// <summary>
/// Request for processing approver action
/// </summary>
public class ApproverActionRequest
{
    public string RequestID { get; set; } = string.Empty; // GUID as string
    public string RequestorEmail { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverEmail { get; set; } = string.Empty;
    public string ApproverAction { get; set; } = string.Empty; // "Approved", "Rejected", "Returned to Requestor"
    public string? Comments { get; set; } // Optional - can be null or empty
    public bool IsTesting { get; set; } = false; // Set to true to skip notifications
}

/// <summary>
/// Response for approver action endpoint
/// </summary>
public class ApproverActionResponse
{
    public string StatusCode { get; set; } = "200";
    public Dictionary<string, string> Headers { get; set; } = new() { { "Content-Type", "application/json" } };
    public ApproverActionResponseBody Body { get; set; } = new();
}

/// <summary>
/// Response body for approver action
/// </summary>
public class ApproverActionResponseBody
{
    public string Message { get; set; } = string.Empty;
    public string? RequestCode { get; set; }
    public string? Action { get; set; }
    public int? CurrentSeqOrder { get; set; }
    public int? MaxSeqOrder { get; set; }
    public string? ResultStatus { get; set; } // "Completed", "MovedToNextApprover", "Rejected", "Returned to Requestor"
}
