using HART.Domain.Entities;

namespace HART.Domain.Interfaces;

public interface ILookupDataRepository
{
    // CW specific
    Task<List<LookupItem>> GetLineOfBusinessesAsync();
    Task<List<LookupItem>> GetProjectThemesAsync();

    // Shared
    Task<List<LookupItem>> GetSWPRolesAsync();
    Task<List<LookupItem>> GetFundingTypesAsync();
    Task<List<LookupItem>> GetCountryListAsync();
    Task<List<LookupItem>> GetPositionStatusesAsync();

    // FTE specific
    Task<List<LookupItem>> GetJobLevelsAsync();
    Task<List<LookupItem>> GetPositionTypesAsync();
    Task<List<LookupItem>> GetRoleTypesAsync();
    Task<List<LookupItem>> GetVacancyTypesAsync();
}
