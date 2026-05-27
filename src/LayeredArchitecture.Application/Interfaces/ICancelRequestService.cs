using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Cancel Request operations
/// </summary>
public interface ICancelRequestService
{
    /// <summary>
    /// Cancel a position request and notify approvers
    /// </summary>
    Task<CancelRequestResponse> CancelRequestAsync(CancelRequestDto request);
}
