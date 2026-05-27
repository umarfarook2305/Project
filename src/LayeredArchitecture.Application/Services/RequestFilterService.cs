using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

public class RequestFilterService : IRequestFilterService
{
    private readonly IRequestFilterRepository _repository;

    public RequestFilterService(IRequestFilterRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<RequestListItemResponse>> GetRequestListAsync(RequestFilterRequest filter)
    {
        // Validate pagination parameters
        if (filter.PageNumber < 1)
        {
            filter.PageNumber = 1;
        }

        if (filter.PageSize < 1)
        {
            filter.PageSize = 10;
        }

        if (filter.PageSize > 100)
        {
            filter.PageSize = 100; // Maximum 100 records per page
        }

        // Validate ViewType if provided
        if (!string.IsNullOrEmpty(filter.ViewType))
        {
            if (filter.ViewType != "MyView" && filter.ViewType != "MyTeam")
            {
                throw new ArgumentException("ViewType must be 'MyView' or 'MyTeam'");
            }

            if (string.IsNullOrEmpty(filter.LoggedInEmail))
            {
                throw new ArgumentException("LoggedInEmail is required when ViewType is specified");
            }
        }

        // Validate Timeline if provided
        if (filter.Timeline != null)
        {
            if (!string.IsNullOrEmpty(filter.Timeline.RangeType))
            {
                if (filter.Timeline.RangeType != "month" && filter.Timeline.RangeType != "custom")
                {
                    throw new ArgumentException("Timeline.RangeType must be 'month' or 'custom'");
                }

                if (filter.Timeline.RangeType == "month" && !filter.Timeline.Value.HasValue)
                {
                    throw new ArgumentException("Timeline.Value is required when RangeType is 'month'");
                }

                if (filter.Timeline.RangeType == "custom")
                {
                    if (!filter.Timeline.StartDate.HasValue || !filter.Timeline.EndDate.HasValue)
                    {
                        throw new ArgumentException("Timeline.StartDate and Timeline.EndDate are required when RangeType is 'custom'");
                    }

                    if (filter.Timeline.StartDate > filter.Timeline.EndDate)
                    {
                        throw new ArgumentException("Timeline.StartDate must be before Timeline.EndDate");
                    }
                }
            }
        }

        return await _repository.GetRequestListAsync(filter);
    }

    public async Task<RequestDetailResponse?> GetRequestDetailAsync(string requestId, string? requestType = null)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("RequestId is required", nameof(requestId));
        }

        if (!Guid.TryParse(requestId, out _))
        {
            throw new ArgumentException("RequestId must be a valid GUID", nameof(requestId));
        }

        // Validate request type if provided
        if (!string.IsNullOrEmpty(requestType))
        {
            if (requestType != "FTE" && requestType != "CW")
            {
                throw new ArgumentException("RequestType must be 'FTE' or 'CW'", nameof(requestType));
            }
        }

        return await _repository.GetRequestDetailAsync(requestId, requestType);
    }
}
