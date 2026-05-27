using HART.Application.DTOs;
using HART.Application.Interfaces;
using HART.Domain.Interfaces;

namespace HART.Application.Services;

public class HierarchyService : IHierarchyService
{
    private readonly IHierarchyRepository _repository;

    public HierarchyService(IHierarchyRepository repository)
    {
        _repository = repository;
    }

    public async Task<HierarchyResponse> GetHierarchyAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }

        var hierarchy = await _repository.GetEmployeeHierarchyAsync(email);

        return new HierarchyResponse
        {
            Status = "success",
            Count = hierarchy.Count,
            Data = hierarchy.Select(h => new HierarchyLevel
            {
                Level = h.Level.ToString(),
                Name = h.Name,
                Email = h.Email,
                Title = h.Title
            }).ToList()
        };
    }
}
