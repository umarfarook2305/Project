using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface ILookupDataService
{
    Task<object> GetLookupDataAsync(string userType);
}
