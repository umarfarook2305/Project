using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Position operations
/// </summary>
public interface IPositionRepository
{
    /// <summary>
    /// Get all positions with user details and job requisitions
    /// </summary>
    Task<PositionResponse> GetPositionsAsync();
}
