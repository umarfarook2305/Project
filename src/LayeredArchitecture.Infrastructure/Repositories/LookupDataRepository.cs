using HART.Domain.Entities;
using HART.Domain.Interfaces;
using HART.Infrastructure.Configuration;
using HART.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace HART.Infrastructure.Repositories;

public class LookupDataRepository : ILookupDataRepository
{
    private readonly HttpClient _httpClient;
    private readonly PowerPlatformSettings _settings;
    private readonly ILogger<LookupDataRepository>? _logger;

    // Cache to avoid multiple API calls
    private LookupDataResponse? _fteCache;
    private LookupDataResponse? _cwCache;
    private DateTime? _fteCacheTime;
    private DateTime? _cwCacheTime;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public LookupDataRepository(
        HttpClient httpClient,
        IOptions<PowerPlatformSettings> settings,
        ILogger<LookupDataRepository>? logger = null)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<LookupDataResponse> GetLookupDataFromPowerPlatformAsync(string userType)
    {
        try
        {
            // Check cache
            if (userType.Equals("FTE", StringComparison.OrdinalIgnoreCase))
            {
                if (_fteCache != null && _fteCacheTime.HasValue && 
                    DateTime.UtcNow - _fteCacheTime.Value < _cacheExpiration)
                {
                    _logger?.LogInformation("Returning cached FTE lookup data");
                    return _fteCache;
                }
            }
            else
            {
                if (_cwCache != null && _cwCacheTime.HasValue && 
                    DateTime.UtcNow - _cwCacheTime.Value < _cacheExpiration)
                {
                    _logger?.LogInformation("Returning cached CW lookup data");
                    return _cwCache;
                }
            }

            var requestBody = new { type = userType };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger?.LogInformation("Calling Power Platform API for user type {UserType}", userType);
            var response = await _httpClient.PostAsync(_settings.GetFullUrl(), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var powerPlatformResponse = JsonSerializer.Deserialize<PowerPlatformResponse<LookupDataResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var data = powerPlatformResponse?.Body ?? new LookupDataResponse();

            // Update cache
            if (userType.Equals("FTE", StringComparison.OrdinalIgnoreCase))
            {
                _fteCache = data;
                _fteCacheTime = DateTime.UtcNow;
            }
            else
            {
                _cwCache = data;
                _cwCacheTime = DateTime.UtcNow;
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error calling Power Platform API for user type {UserType}", userType);
            throw;
        }
    }

    public async Task<List<LookupItem>> GetJobLevelsAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.Joblevel?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetSWPRolesAsync()
    {
        // This is called for both FTE and CW, using FTE for consistency
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.SWPRole.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList();
    }

    public async Task<List<LookupItem>> GetPositionTypesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.PositionType?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetFundingTypesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.FundingType.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList();
    }

    public async Task<List<LookupItem>> GetRoleTypesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.RoleType?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetCountryListAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.CountryList.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList();
    }

    public async Task<List<LookupItem>> GetPositionStatusesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.Status.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList();
    }

    public async Task<List<LookupItem>> GetVacancyTypesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("FTE");
        return data.StatusOfPosition?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetLineOfBusinessesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("CW");
        return data.LineOfBusiness?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetProjectThemesAsync()
    {
        var data = await GetLookupDataFromPowerPlatformAsync("CW");
        return data.ProjectTheme?.Select(x => new LookupItem 
        { 
            Value = x.Value, 
            Label = x.Label 
        }).ToList() ?? new List<LookupItem>();
    }
}
