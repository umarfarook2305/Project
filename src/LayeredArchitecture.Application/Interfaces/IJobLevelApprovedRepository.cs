using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Job Level Approved operations
/// </summary>
public interface IJobLevelApprovedRepository
{
    /// <summary>
    /// Get approved positions grouped by job level from database
    /// </summary>
    Task<JobLevelApprovedResponse> GetJobLevelApprovedAsync(JobLevelApprovedRequest request);
}
