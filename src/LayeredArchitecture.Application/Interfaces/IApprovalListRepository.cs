using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Approval List operations
/// </summary>
public interface IApprovalListRepository
{
    /// <summary>
    /// Get approval list with comprehensive filters and pagination from database
    /// </summary>
    Task<ApprovalListResponse> GetApprovalListAsync(ApprovalListRequest request);
}
