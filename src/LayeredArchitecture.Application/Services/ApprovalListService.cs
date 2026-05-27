using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Approval List operations
/// </summary>
public class ApprovalListService : IApprovalListService
{
    private readonly IApprovalListRepository _repository;

    public ApprovalListService(IApprovalListRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApprovalListResponse> GetApprovalListAsync(ApprovalListRequest request)
    {
        // Validate input
        if (request.ListTable != "Approval")
        {
            throw new ArgumentException("Invalid List Table Name. Expected 'Approval'.", nameof(request.ListTable));
        }

        if (request.Approver == null || request.Approver.Count == 0)
        {
            throw new ArgumentException("Approver list cannot be empty", nameof(request.Approver));
        }

        if (request.Count <= 0)
        {
            throw new ArgumentException("Count must be greater than 0", nameof(request.Count));
        }

        // Validate pagination parameters
        if (request.PageNumber < 1)
        {
            throw new ArgumentException("PageNumber must be greater than 0", nameof(request.PageNumber));
        }

        if (request.PageSize < 1)
        {
            throw new ArgumentException("PageSize must be greater than 0", nameof(request.PageSize));
        }

        if (request.PageSize > 1000)
        {
            throw new ArgumentException("PageSize cannot exceed 1000", nameof(request.PageSize));
        }

        // Validate ViewType if provided
        if (!string.IsNullOrEmpty(request.ViewType) && 
            request.ViewType != "MyView" && request.ViewType != "MyTeam")
        {
            throw new ArgumentException("ViewType must be 'MyView' or 'MyTeam'", nameof(request.ViewType));
        }

        // Validate Timeline if provided
        if (request.Timeline != null)
        {
            if (request.Timeline.RangeType != "month" && request.Timeline.RangeType != "custom")
            {
                throw new ArgumentException("Timeline RangeType must be 'month' or 'custom'", nameof(request.Timeline));
            }

            if (request.Timeline.RangeType == "month" && !request.Timeline.Value.HasValue)
            {
                throw new ArgumentException("Timeline Value is required when RangeType is 'month'", nameof(request.Timeline));
            }

            if (request.Timeline.RangeType == "custom" && 
                (!request.Timeline.StartDate.HasValue || !request.Timeline.EndDate.HasValue))
            {
                throw new ArgumentException("Timeline StartDate and EndDate are required when RangeType is 'custom'", nameof(request.Timeline));
            }
        }

        return await _repository.GetApprovalListAsync(request);
    }
}
