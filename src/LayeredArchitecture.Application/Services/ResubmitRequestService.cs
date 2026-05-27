using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

public class ResubmitRequestService : IResubmitRequestService
{
    private readonly IResubmitRequestRepository _repository;

    public ResubmitRequestService(IResubmitRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResubmitResponse> ResubmitRequestAsync(ResubmitRequest request)
    {
        // Validate RequestId
        if (string.IsNullOrWhiteSpace(request.Request?.RequestId))
            throw new ArgumentException("RequestId is required");

        if (!Guid.TryParse(request.Request.RequestId, out _))
            throw new ArgumentException("RequestId must be a valid GUID");

        // Validate approvals array exists
        if (request.Approvals == null)
            throw new ArgumentException("Approvals array is required");

        return await _repository.ResubmitRequestAsync(request);
    }
}
