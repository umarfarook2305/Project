using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Approval List operations
/// </summary>
public interface IApprovalListService
{
    /// <summary>
    /// Get approval list with filters
    /// </summary>
    Task<ApprovalListResponse> GetApprovalListAsync(ApprovalListRequest request);
}
