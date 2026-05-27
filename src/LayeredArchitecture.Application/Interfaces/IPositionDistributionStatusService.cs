using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Position Distribution Status operations
/// </summary>
public interface IPositionDistributionStatusService
{
    /// <summary>
    /// Get approved requests distribution over time periods
    /// </summary>
    Task<PositionDistributionStatusResponse> GetPositionDistributionStatusAsync(PositionDistributionStatusRequest request);
}
