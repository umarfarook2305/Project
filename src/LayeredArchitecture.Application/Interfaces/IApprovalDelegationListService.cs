using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Approval Delegation List operations
/// </summary>
public interface IApprovalDelegationListService
{
    /// <summary>
    /// Get delegated approval list with pagination
    /// </summary>
    Task<ApprovalDelegationListResponse> GetApprovalDelegationListAsync(ApprovalDelegationListRequest request);
}
