using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Cancel Request operations
/// </summary>
public class CancelRequestService : ICancelRequestService
{
    private readonly ICancelRequestRepository _repository;

    public CancelRequestService(ICancelRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<CancelRequestResponse> CancelRequestAsync(CancelRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.RequestID))
            throw new ArgumentException("RequestID is required", nameof(request.RequestID));

        if (string.IsNullOrWhiteSpace(request.RequestorName))
            throw new ArgumentException("RequestorName is required", nameof(request.RequestorName));

        if (string.IsNullOrWhiteSpace(request.RequestorEmail))
            throw new ArgumentException("RequestorEmail is required", nameof(request.RequestorEmail));

        return await _repository.CancelRequestAsync(request);
    }
}
