using HART.Domain.Entities;

namespace HART.Domain.Interfaces;

public interface IHierarchyRepository
{
    /// <summary>
    /// Get management hierarchy for an employee by email
    /// </summary>
    /// <param name="email">Employee email address</param>
    /// <returns>List of managers in hierarchical order</returns>
    Task<List<EmployeeHierarchy>> GetEmployeeHierarchyAsync(string email);
}
