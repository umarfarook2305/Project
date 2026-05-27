using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IHierarchyService
{
    /// <summary>
    /// Get employee management hierarchy by email
    /// </summary>
    /// <param name="email">Employee email address</param>
    /// <returns>Hierarchy response with status, count, and data</returns>
    Task<HierarchyResponse> GetHierarchyAsync(string email);
}
