using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Get Delegate List operations
/// </summary>
public interface IGetDelegateListRepository
{
    /// <summary>
    /// Get list of delegations for a user from database
    /// </summary>
    Task<GetDelegateListResponse> GetDelegateListAsync(GetDelegateListRequest request);
}
