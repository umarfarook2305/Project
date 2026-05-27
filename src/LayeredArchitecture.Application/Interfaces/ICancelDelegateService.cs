using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Cancel Delegate operations
/// </summary>
public interface ICancelDelegateService
{
    /// <summary>
    /// Cancel an approval delegation
    /// </summary>
    Task<CancelDelegateResponse> CancelDelegateAsync(CancelDelegateRequest request);
}
