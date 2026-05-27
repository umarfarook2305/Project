using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Pending Status Distribution operations
/// </summary>
public interface IPendingStatusDistributionRepository
{
    /// <summary>
    /// Get pending approvals grouped by requester role from database
    /// </summary>
    Task<PendingStatusDistributionResponse> GetPendingStatusDistributionAsync(PendingStatusDistributionRequest request);
}
