using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Cancel Request operations
/// </summary>
public interface ICancelRequestRepository
{
    /// <summary>
    /// Cancel a position request and notify approvers
    /// </summary>
    Task<CancelRequestResponse> CancelRequestAsync(CancelRequestDto request);
}
