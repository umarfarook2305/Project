using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Pending Status Distribution operations
/// </summary>
public interface IPendingStatusDistributionService
{
    /// <summary>
    /// Get pending approvals grouped by requester role
    /// </summary>
    Task<PendingStatusDistributionResponse> GetPendingStatusDistributionAsync(PendingStatusDistributionRequest request);
}
