using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IResubmitRequestService
{
    Task<ResubmitResponse> ResubmitRequestAsync(ResubmitRequest request);
}
