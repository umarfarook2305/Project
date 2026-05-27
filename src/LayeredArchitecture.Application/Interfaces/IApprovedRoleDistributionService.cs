using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Approved Role Distribution operations
/// </summary>
public interface IApprovedRoleDistributionService
{
    /// <summary>
    /// Get approved FTE positions grouped by role type
    /// </summary>
    Task<ApprovedRoleDistributionResponse> GetApprovedRoleDistributionAsync(ApprovedRoleDistributionRequest request);
}
