using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Approval Delegation List operations
/// </summary>
public interface IApprovalDelegationListRepository
{
    /// <summary>
    /// Get delegated approval list with pagination from database
    /// </summary>
    Task<ApprovalDelegationListResponse> GetApprovalDelegationListAsync(ApprovalDelegationListRequest request);
}
