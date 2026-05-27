using HART.Application.DTOs;
using HART.Application.Interfaces;
using HART.Domain.Interfaces;

namespace HART.Application.Services;

public class LookupDataService : ILookupDataService
{
    private readonly ILookupDataRepository _lookupDataRepository;

    public LookupDataService(ILookupDataRepository lookupDataRepository)
    {
        _lookupDataRepository = lookupDataRepository;
    }

    public async Task<object> GetLookupDataAsync(string userType)
    {
        // Validate user type - only CW and FTE are allowed
        if (string.IsNullOrWhiteSpace(userType))
        {
            throw new ArgumentException("User type is required", nameof(userType));
        }

        var normalizedType = userType.Trim().ToUpperInvariant();

        if (normalizedType != "CW" && normalizedType != "FTE")
        {
            throw new ArgumentException($"Invalid user type '{userType}'. Only 'CW' or 'FTE' are allowed.", nameof(userType));
        }

        if (normalizedType == "CW")
        {
            return await GetCWLookupDataAsync();
        }
        else // FTE
        {
            return await GetFTELookupDataAsync();
        }
    }

    private async Task<CWLookupDataDto> GetCWLookupDataAsync()
    {
        var lineOfBusinesses = await _lookupDataRepository.GetLineOfBusinessesAsync();
        var projectThemes = await _lookupDataRepository.GetProjectThemesAsync();
        var swpRoles = await _lookupDataRepository.GetSWPRolesAsync();
        var fundingTypes = await _lookupDataRepository.GetFundingTypesAsync();
        var countryList = await _lookupDataRepository.GetCountryListAsync();
        var statuses = await _lookupDataRepository.GetPositionStatusesAsync();

        return new CWLookupDataDto
        {
            LineOfBusiness = lineOfBusinesses.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            ProjectTheme = projectThemes.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            SWPRole = swpRoles.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            FundingType = fundingTypes.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            CountryList = countryList.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            Status = statuses.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList()
        };
    }

    private async Task<FTELookupDataDto> GetFTELookupDataAsync()
    {
        var jobLevels = await _lookupDataRepository.GetJobLevelsAsync();
        var swpRoles = await _lookupDataRepository.GetSWPRolesAsync();
        var positionTypes = await _lookupDataRepository.GetPositionTypesAsync();
        var fundingTypes = await _lookupDataRepository.GetFundingTypesAsync();
        var roleTypes = await _lookupDataRepository.GetRoleTypesAsync();
        var countryList = await _lookupDataRepository.GetCountryListAsync();
        var statuses = await _lookupDataRepository.GetPositionStatusesAsync();
        var statusOfPosition = await _lookupDataRepository.GetVacancyTypesAsync();

        return new FTELookupDataDto
        {
            Joblevel = jobLevels.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            SWPRole = swpRoles.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            PositionType = positionTypes.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            FundingType = fundingTypes.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            RoleType = roleTypes.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            CountryList = countryList.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            Status = statuses.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList(),
            StatusOfPosition = statusOfPosition.Select(x => new LookupItemDto { Value = x.Value, Label = x.Label }).ToList()
        };
    }
}
