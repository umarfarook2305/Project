using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Approved Role Distribution operations
/// </summary>
public interface IApprovedRoleDistributionRepository
{
    /// <summary>
    /// Get approved FTE positions grouped by role type from database
    /// </summary>
    Task<ApprovedRoleDistributionResponse> GetApprovedRoleDistributionAsync(ApprovedRoleDistributionRequest request);
}
