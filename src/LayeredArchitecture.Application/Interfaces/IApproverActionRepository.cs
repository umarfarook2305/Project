using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Approver Action operations
/// </summary>
public interface IApproverActionRepository
{
    /// <summary>
    /// Process approver action in database
    /// </summary>
    Task<ApproverActionResponse> ProcessApproverActionAsync(ApproverActionRequest request);
}
