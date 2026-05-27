using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IResubmitRequestRepository
{
    Task<ResubmitResponse> ResubmitRequestAsync(ResubmitRequest request);
}
