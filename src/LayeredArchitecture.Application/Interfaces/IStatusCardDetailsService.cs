using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Status Card Details operations
/// </summary>
public interface IStatusCardDetailsService
{
    /// <summary>
    /// Get dashboard statistics (total, FTE, CW, approved, pending, rejected counts)
    /// </summary>
    Task<StatusCardDetailsResponse> GetStatusCardDetailsAsync(StatusCardDetailsRequest request);
}
