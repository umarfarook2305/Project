using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Hiring Trend Data operations
/// </summary>
public interface IHiringTrendDataService
{
    /// <summary>
    /// Get hiring trend data (FTE and CW counts over 6 time buckets)
    /// </summary>
    Task<HiringTrendDataResponse> GetHiringTrendDataAsync(HiringTrendDataRequest request);
}
