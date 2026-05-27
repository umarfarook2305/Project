using HART.Domain.Entities;

namespace HART.Domain.Interfaces;

public interface IJobProfileRepository
{
    /// <summary>
    /// Get job profiles, optionally filtered by job level
    /// </summary>
    /// <param name="jobLevel">Optional job level filter (name, ID, or code)</param>
    /// <returns>List of job profiles</returns>
    Task<List<JobProfile>> GetJobProfilesAsync(string? jobLevel = null);
}
