namespace HART.Application.DTOs;

/// <summary>
/// Request for getting delegated approval list
/// </summary>
public class ApprovalDelegationListRequest
{
    public string CurrentUserEmail { get; set; } = string.Empty;
    public bool IsPending { get; set; } = true;

    // Pagination parameters
    public int PageNumber { get; set; } = 1; // Page number (1-based)
    public int PageSize { get; set; } = 10; // Records per page
}

/// <summary>
/// Response for delegated approval list endpoint with pagination
/// </summary>
public class ApprovalDelegationListResponse
{
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string> Headers { get; set; } = new() { { "Content-Type", "application/json" } };
    public List<ApprovalDelegationListItem> Body { get; set; } = new();
    public PaginationMetadata? Pagination { get; set; }
}

/// <summary>
/// Individual delegated approval item
/// </summary>
public class ApprovalDelegationListItem
{
    public Guid? RequestUniqueId { get; set; }
    public string? RequestId { get; set; }
    public string? Status { get; set; }
    public string? Requestor { get; set; }
    public string? Funding { get; set; }
    public string? Type { get; set; }
    public string? Level { get; set; }
    public DateTime? SubmittedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public string? Delegator { get; set; } // The person who delegated to current user
}
