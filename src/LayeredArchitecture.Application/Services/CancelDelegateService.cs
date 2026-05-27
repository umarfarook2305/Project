using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Cancel Delegate operations
/// </summary>
public class CancelDelegateService : ICancelDelegateService
{
    private readonly ICancelDelegateRepository _repository;

    public CancelDelegateService(ICancelDelegateRepository repository)
    {
        _repository = repository;
    }

    public async Task<CancelDelegateResponse> CancelDelegateAsync(CancelDelegateRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.CancelDelegateAsync(request);
    }

    private static void ValidateRequest(CancelDelegateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DelegateID))
        {
            throw new ArgumentException("DelegateID is required");
        }

        if (!Guid.TryParse(request.DelegateID, out _))
        {
            throw new ArgumentException("DelegateID must be a valid GUID");
        }

        if (string.IsNullOrWhiteSpace(request.CancelledOn))
        {
            throw new ArgumentException("CancelledOn date is required");
        }

        if (!DateTime.TryParse(request.CancelledOn, out _))
        {
            throw new ArgumentException("CancelledOn must be a valid date");
        }
    }
}
