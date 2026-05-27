using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Get Delegate List operations
/// </summary>
public class GetDelegateListService : IGetDelegateListService
{
    private readonly IGetDelegateListRepository _repository;

    public GetDelegateListService(IGetDelegateListRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetDelegateListResponse> GetDelegateListAsync(GetDelegateListRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.GetDelegateListAsync(request);
    }

    private static void ValidateRequest(GetDelegateListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailId))
        {
            throw new ArgumentException("EmailId is required");
        }

        // DelegationStatus is a boolean, no validation needed beyond type checking
    }
}
