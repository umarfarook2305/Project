namespace HART.Application.DTOs;

/// <summary>
/// Request for getting approval list with comprehensive filters and pagination
/// </summary>
public class ApprovalListRequest
{
    public string ListTable { get; set; } = "Approval";
    public List<string> Approver { get; set; } = new();
    public string? LastId { get; set; }
    public bool IsPending { get; set; } = true;
    public int Count { get; set; } = 5000; // Legacy - max records (backward compatible)

    // Pagination parameters
    public int PageNumber { get; set; } = 1; // Page number (1-based)
    public int PageSize { get; set; } = 10; // Records per page

    // Additional filters from getDataByFilter_Request
    public string? LoggedInEmail { get; set; }
    public string? ViewType { get; set; } // "MyView" or "MyTeam"
    public string? PendingWith { get; set; }
    public string? PositionType { get; set; } // Job profile name or GUID
    public string? Type { get; set; } // Request type: "FTE" or "CW"
    public string? Funding { get; set; } // Funding type name or GUID
    public string? Level { get; set; } // Job level name
    public List<string>? Requestor { get; set; } // Array of requestor emails
    public TimelineFilter? Timeline { get; set; } // Uses shared TimelineFilter from RequestFilterDto
}

/// <summary>
/// Response for approval list endpoint with pagination
/// </summary>
public class ApprovalListResponse
{
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string> Headers { get; set; } = new() { { "Content-Type", "application/json" } };
    public List<ApprovalListItem> Body { get; set; } = new();
    public PaginationMetadata? Pagination { get; set; } // Pagination metadata
}

/// <summary>
/// Individual approval list item
/// </summary>
public class ApprovalListItem
{
    public Guid? RequestUniqueId { get; set; }
    public string? RequestId { get; set; }
    public string? Status { get; set; }
    public string? Requestor { get; set; }
    public string? PendingWith { get; set; }
    public string? Position { get; set; }
    public string? Funding { get; set; }
    public string? Type { get; set; }
    public string? Level { get; set; }
    public DateTime? SubmittedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public string? ApproverName { get; set; }
}
