using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Approval Delegation List operations
/// </summary>
public class ApprovalDelegationListService : IApprovalDelegationListService
{
    private readonly IApprovalDelegationListRepository _repository;

    public ApprovalDelegationListService(IApprovalDelegationListRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApprovalDelegationListResponse> GetApprovalDelegationListAsync(ApprovalDelegationListRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.GetApprovalDelegationListAsync(request);
    }

    private static void ValidateRequest(ApprovalDelegationListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentUserEmail))
        {
            throw new ArgumentException("CurrentUserEmail is required");
        }

        if (request.PageNumber < 1)
        {
            throw new ArgumentException("PageNumber must be greater than or equal to 1");
        }

        if (request.PageSize < 1 || request.PageSize > 1000)
        {
            throw new ArgumentException("PageSize must be between 1 and 1000");
        }
    }
}
