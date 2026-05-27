namespace HART.Application.DTOs;

/// <summary>
/// Request object for resubmission
/// </summary>
public class ResubmitRequestData
{
    public string? JobProfileId { get; set; }
    public string? LineOfBusinessId { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectThemeId { get; set; }
    public string? FundingTypeId { get; set; }
    public string? SwpRoleId { get; set; }
    public string? JobFamilyId { get; set; }
    public string? CountryId { get; set; }
    public string? CityId { get; set; }
    public string? Rationale { get; set; }
    public string? CurrentStatusId { get; set; }
    public string? RequestType { get; set; }
    public bool? IsAiDataRole { get; set; }
    public bool? IsApplicationEngineeringInvestment { get; set; }
    public bool? IsAiGovernanceInvestment { get; set; }
    public bool? IsCwConversionFuture { get; set; }
    public string? RequestedOn { get; set; }
    public string? PendingWithName { get; set; }
    public string? PendingWithEmail { get; set; }

    /// <summary>
    /// CurrentStep is accepted but not persisted to database (workflow tracking only)
    /// </summary>
    public int? CurrentStep { get; set; }

    public string? RequestedByEmail { get; set; }
    public string? RequestedByName { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string? JobLevelId { get; set; }
    public string? PositionTypeId { get; set; }
    public string? PositionId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? VacancyTypeId { get; set; }
    public string? PromotedEmployeeEmailId { get; set; }
    public string? ReplacementEmployeeEmailId { get; set; }
    public string? CwEmployeeEmailId { get; set; }
    public decimal? CwAnnualRate { get; set; }
    public decimal? CwHourlyRate { get; set; }
    public bool? HasNoIntraLevelReporting { get; set; }
    public bool? IsNewPostion { get; set; }
    public bool? MeetsSpanOfControlRequirements { get; set; }
}

/// <summary>
/// Approval record for resubmission
/// </summary>
public class ResubmitApprovalData
{
    public string? PositionStatusId { get; set; }
    public string? RequestedTo { get; set; }
    public string? RequestedToEmail { get; set; }
    public int? SequenceOrder { get; set; }
    public bool? IsApprover { get; set; }
    public string? RequestedToRole { get; set; }
    public string? ApprovedOn { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RequestedOn { get; set; }
}

/// <summary>
/// Request to resubmit with updated data
/// </summary>
public class ResubmitRequest
{
    public ResubmitRequestData Request { get; set; } = new();
    public List<ResubmitApprovalData> Approvals { get; set; } = new();
}

/// <summary>
/// Response for resubmit operation
/// </summary>
public class ResubmitResponse
{
    public string StatusCode { get; set; } = "200";
    public ResubmitBody Body { get; set; } = new();
}

/// <summary>
/// Body of resubmit response
/// </summary>
public class ResubmitBody
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public string? RequestId { get; set; }
}
