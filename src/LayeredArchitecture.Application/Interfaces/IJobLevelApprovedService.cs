using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Job Level Approved operations
/// </summary>
public interface IJobLevelApprovedService
{
    /// <summary>
    /// Get approved positions grouped by job level
    /// </summary>
    Task<JobLevelApprovedResponse> GetJobLevelApprovedAsync(JobLevelApprovedRequest request);
}
