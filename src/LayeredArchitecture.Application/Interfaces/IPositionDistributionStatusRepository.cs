using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Position Distribution Status operations
/// </summary>
public interface IPositionDistributionStatusRepository
{
    /// <summary>
    /// Get approved requests distribution over time periods from database
    /// </summary>
    Task<PositionDistributionStatusResponse> GetPositionDistributionStatusAsync(PositionDistributionStatusRequest request);
}
