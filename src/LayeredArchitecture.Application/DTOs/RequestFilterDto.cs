namespace HART.Application.DTOs;

/// <summary>
/// Request filter for listing/searching requests
/// </summary>
public class RequestFilterRequest
{
    public string? RequestId { get; set; }
    public string? LoggedInEmail { get; set; }
    public string? ViewType { get; set; } // "MyView" or "MyTeam"
    public string? PendingWith { get; set; }
    public string? PositionType { get; set; }
    public string? Type { get; set; } // "FTE" or "CW"
    public string? Funding { get; set; }
    public string? Level { get; set; }
    public List<string>? Requestor { get; set; }
    public bool? IsPending { get; set; }
    public TimelineFilter? Timeline { get; set; }

    // Pagination parameters
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMetadata
{
    public int TotalRecords { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Timeline filter for date range
/// </summary>
public class TimelineFilter
{
    public string? RangeType { get; set; } // "month" or "custom"
    public int? Value { get; set; } // Number of months (for "month" rangeType)
    public DateTime? StartDate { get; set; } // For "custom" rangeType
    public DateTime? EndDate { get; set; } // For "custom" rangeType
}

/// <summary>
/// Request list item response
/// </summary>
public class RequestListItemResponse
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Requestor { get; set; } = string.Empty;
    public string PendingWith { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Funding { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime? SubmittedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public string UniqueID { get; set; } = string.Empty;
}

/// <summary>
/// Detailed request response (FTE or CW)
/// </summary>
public class RequestDetailResponse
{
    public string RequestType { get; set; } = string.Empty;
    public LabelValuePair? JobProfile { get; set; }
    public LabelValuePair? PositionType { get; set; }
    public LabelValuePair? JobLevel { get; set; }
    public LabelValuePair? Funding { get; set; }
    public LabelValuePair? StatusOfPosition { get; set; }
    public LabelValuePair? PositionId { get; set; }
    public string? OpenJobRequisition { get; set; }
    public string? StaffingStatus { get; set; }
    public LabelValuePair? RoleType { get; set; }
    public string? ProjectName { get; set; }
    public LabelValuePair? SWPRole { get; set; }
    public LabelValuePair? JobFamily { get; set; }
    public bool AI_or_DataRole { get; set; }
    public LabelValuePair? Country { get; set; }
    public string? City { get; set; }
    public string? Rationale { get; set; }
    public List<ApproverDetail> Approvers { get; set; } = new();
    public List<ApproverChainActivity> ApproverChainActivity { get; set; } = new();
    public object? CurrentStatus { get; set; } // Can be string or LabelValuePair
    public DateTime? CompletedOn { get; set; }

    // FTE specific
    public decimal? CWAnnualRate { get; set; }
    public decimal? CWHourlyRate { get; set; }
    public bool? HasNoIntraLevelReporting { get; set; }
    public bool? MeetsSpanofControlRequirements { get; set; }
    public EmployeeDetail? ReplacementEmployee { get; set; }
    public EmployeeDetail? PromotedEmployee { get; set; }
    public EmployeeDetail? CWConversionEmployee { get; set; }

    // CW specific
    public LabelValuePair? LineOfBusiness { get; set; }
    public LabelValuePair? ProjectTheme { get; set; }
    public bool? CwConversionInFuture { get; set; }
    public bool? InverstmentInGoveranceAndAI { get; set; }
    public bool? ApplicationEngineering { get; set; }
    public bool? IsApplicationEngineeringInvestment { get; set; }
    public bool? IsAiGovernanceInvestment { get; set; }
    public bool? IsCwConversionFuture { get; set; }

    public List<CommentDetail> CommentsList { get; set; } = new();
}

public class LabelValuePair
{
    public string? Label { get; set; }
    public string? Value { get; set; }
}

public class ApproverDetail
{
    public string UniqueID { get; set; } = string.Empty;
    public string RequestToName { get; set; } = string.Empty;
    public string RequestToEmail { get; set; } = string.Empty;
    public string RequestToRole { get; set; } = string.Empty;
    public string SeqOrder { get; set; } = string.Empty;
}

public class ApproverChainActivity
{
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverStatus { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedOn { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledBy { get; set; }
    public string ApproverRole { get; set; } = string.Empty;
    public string SeqOrder { get; set; } = string.Empty;
    public DateTime? RequestedOn { get; set; }
    public bool IsBehalfof { get; set; }
    public string? ReturnedBy { get; set; }
    public DateTime? ReturnedOn { get; set; }
}

public class EmployeeDetail
{
    public string? UniqueId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? EmpId { get; set; }
}

public class CommentDetail
{
    public string ApproverName { get; set; } = string.Empty;
    public DateTime? CommentedOn { get; set; }
    public string ApproverStatus { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}
