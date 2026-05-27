using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IUpdateDelegateRepository
{
    Task<UpdateDelegateResponse> UpdateDelegateAsync(UpdateDelegateRequest request);
}
