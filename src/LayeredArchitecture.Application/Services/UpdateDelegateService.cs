using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

public class UpdateDelegateService : IUpdateDelegateService
{
    private readonly IUpdateDelegateRepository _repository;

    public UpdateDelegateService(IUpdateDelegateRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateDelegateResponse> UpdateDelegateAsync(UpdateDelegateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DelegateId))
            throw new ArgumentException("DelegateId is required");

        if (!Guid.TryParse(request.DelegateId, out _))
            throw new ArgumentException("DelegateId must be a valid GUID");

        return await _repository.UpdateDelegateAsync(request);
    }
}
