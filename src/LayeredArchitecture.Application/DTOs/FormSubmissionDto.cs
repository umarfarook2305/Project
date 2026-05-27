namespace HART.Application.DTOs;

/// <summary>
/// Main request for form submission (FTE or CW)
/// </summary>
public class FormSubmissionRequest
{
    public RequestDetails Request { get; set; } = new();
    public List<ApprovalDetails> Approvals { get; set; } = new();
}

/// <summary>
/// Request details section
/// </summary>
public class RequestDetails
{
    // Common fields for both FTE and CW
    public string? JobProfileId { get; set; }
    public string? JobFamilyId { get; set; }
    public string FundingTypeId { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public string CityId { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string CurrentStatusId { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string RequestType { get; set; } = string.Empty; // "FTE" or "CW"
    public DateTime RequestedOn { get; set; }
    public string PendingWithName { get; set; } = string.Empty;
    public string PendingWithEmail { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public string RequestedByEmail { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public bool IsAiDataRole { get; set; }

    // FTE-specific fields
    public string? JobLevelId { get; set; }
    public string? PositionTypeId { get; set; }
    public string? StatusOfPositionId { get; set; }
    public string? RoleTypeId { get; set; }
    public bool? IsNewPosition { get; set; }
    public string? PositionId { get; set; }

    // CW-specific fields
    public string? SwpRoleId { get; set; }
    public string? LineOfBusinessId { get; set; }
    public string? ProjectThemeId { get; set; }
    public bool? IsApplicationEngineeringInvestment { get; set; }
    public bool? IsAiGovernanceInvestment { get; set; }
    public bool? IsCwConversionFuture { get; set; }

    // Optional fields for specific scenarios
    public decimal? CWHourlyRate { get; set; }
    public decimal? CWAnnualRate { get; set; }
    public string? CWEmployeeEmailId { get; set; }
    public string? PromotedEmployeeEmailId { get; set; }
    public string? ReplacementEmployeeEmailId { get; set; }
    public bool? HasNoIntraLevelReporting { get; set; }
    public bool? MeetsSpanOfControlRequirements { get; set; }
}

/// <summary>
/// Approval details for each approval step
/// </summary>
public class ApprovalDetails
{
    public string PositionStatusId { get; set; } = string.Empty;
    public string RequestedTo { get; set; } = string.Empty;
    public string RequestedToEmail { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public bool IsApprover { get; set; }
    public string RequestedToRole { get; set; } = string.Empty;
    public DateTime? ApprovedOn { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? RequestedOn { get; set; }
}

/// <summary>
/// Response from form submission
/// </summary>
public class FormSubmissionResponse
{
    public string Status { get; set; } = "success";
    public string RequestId { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string FailedStep { get; set; } = string.Empty;
}
