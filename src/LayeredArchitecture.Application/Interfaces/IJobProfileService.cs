using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IJobProfileService
{
    /// <summary>
    /// Get job profiles based on optional job level filter
    /// </summary>
    /// <param name="jobLevel">Optional job level filter</param>
    /// <returns>List of job profile responses</returns>
    Task<List<JobProfileResponse>> GetJobProfilesAsync(string? jobLevel = null);
}
