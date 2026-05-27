using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Job Family Distribution operations
/// </summary>
public interface IJobFamilyDistributionService
{
    /// <summary>
    /// Get approved positions grouped by job family
    /// </summary>
    Task<JobFamilyDistributionResponse> GetJobFamilyDistributionAsync(JobFamilyDistributionRequest request);
}
