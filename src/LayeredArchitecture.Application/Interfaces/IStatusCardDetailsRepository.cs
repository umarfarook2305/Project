using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Status Card Details operations
/// </summary>
public interface IStatusCardDetailsRepository
{
    /// <summary>
    /// Get dashboard statistics from database
    /// </summary>
    Task<StatusCardDetailsResponse> GetStatusCardDetailsAsync(StatusCardDetailsRequest request);
}
