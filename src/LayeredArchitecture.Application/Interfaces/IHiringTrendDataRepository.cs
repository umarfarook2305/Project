using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Hiring Trend Data operations
/// </summary>
public interface IHiringTrendDataRepository
{
    /// <summary>
    /// Get hiring trend data (FTE and CW counts over 6 time buckets) from database
    /// </summary>
    Task<HiringTrendDataResponse> GetHiringTrendDataAsync(HiringTrendDataRequest request);
}
