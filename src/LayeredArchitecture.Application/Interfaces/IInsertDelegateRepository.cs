using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IInsertDelegateRepository
{
    Task<InsertDelegateResponse> InsertDelegateAsync(InsertDelegateRequest request);
}
