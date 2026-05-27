using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IUpdateDelegateService
{
    Task<UpdateDelegateResponse> UpdateDelegateAsync(UpdateDelegateRequest request);
}
