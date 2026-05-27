using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Cancel Delegate operations
/// </summary>
public interface ICancelDelegateRepository
{
    /// <summary>
    /// Cancel an approval delegation in the database
    /// </summary>
    Task<CancelDelegateResponse> CancelDelegateAsync(CancelDelegateRequest request);
}
