using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Approver Action operations
/// </summary>
public interface IApproverActionService
{
    /// <summary>
    /// Process approver action (Approve/Reject/Return)
    /// </summary>
    Task<ApproverActionResponse> ProcessApproverActionAsync(ApproverActionRequest request);
}
