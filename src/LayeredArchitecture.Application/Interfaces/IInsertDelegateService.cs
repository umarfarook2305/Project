using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IInsertDelegateService
{
    Task<InsertDelegateResponse> InsertDelegateAsync(InsertDelegateRequest request);
}
