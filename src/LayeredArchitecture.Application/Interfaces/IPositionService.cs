using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Position operations
/// </summary>
public interface IPositionService
{
    /// <summary>
    /// Get all positions with user details and job requisitions
    /// </summary>
    Task<PositionResponse> GetPositionsAsync();
}
