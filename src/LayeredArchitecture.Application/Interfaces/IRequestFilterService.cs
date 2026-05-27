using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IRequestFilterService
{
    /// <summary>
    /// Get paginated list of requests with optional filtering
    /// </summary>
    Task<PaginatedResponse<RequestListItemResponse>> GetRequestListAsync(RequestFilterRequest filter);

    /// <summary>
    /// Get detailed request information by ID
    /// </summary>
    Task<RequestDetailResponse?> GetRequestDetailAsync(string requestId, string? requestType = null);
}
