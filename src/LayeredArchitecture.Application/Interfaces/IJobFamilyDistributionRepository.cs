using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Job Family Distribution operations
/// </summary>
public interface IJobFamilyDistributionRepository
{
    /// <summary>
    /// Get approved positions grouped by job family from database
    /// </summary>
    Task<JobFamilyDistributionResponse> GetJobFamilyDistributionAsync(JobFamilyDistributionRequest request);
}
