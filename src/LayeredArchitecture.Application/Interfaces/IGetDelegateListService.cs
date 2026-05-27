using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Get Delegate List operations
/// </summary>
public interface IGetDelegateListService
{
    /// <summary>
    /// Get list of delegations for a user
    /// </summary>
    Task<GetDelegateListResponse> GetDelegateListAsync(GetDelegateListRequest request);
}
