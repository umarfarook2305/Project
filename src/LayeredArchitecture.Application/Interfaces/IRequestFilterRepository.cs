using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IRequestFilterRepository
{
    Task<PaginatedResponse<RequestListItemResponse>> GetRequestListAsync(RequestFilterRequest filter);
    Task<RequestDetailResponse?> GetRequestDetailAsync(string requestId, string? requestType);
}
