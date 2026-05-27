using HART.Domain.Entities;
using HART.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace HART.Infrastructure.Repositories;

public class SqlLookupDataRepository : ILookupDataRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlLookupDataRepository>? _logger;

    // In-memory caches
    private Dictionary<string, List<LookupItem>>? _cwCache;
    private Dictionary<string, List<LookupItem>>? _fteCache;
    private DateTime? _cwCacheTime;
    private DateTime? _fteCacheTime;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public SqlLookupDataRepository(
        IConfiguration configuration,
        ILogger<SqlLookupDataRepository>? logger = null)
    {
        _connectionString = configuration.GetConnectionString("HARTDatabase")
            ?? throw new InvalidOperationException("Connection string 'HARTDatabase' not found.");
        _logger = logger;
    }

    private async Task<Dictionary<string, List<LookupItem>>> GetCWLookupDataFromDatabaseAsync()
    {
        try
        {
            // Check cache
            if (_cwCache != null && _cwCacheTime.HasValue &&
                DateTime.UtcNow - _cwCacheTime.Value < _cacheExpiration)
            {
                _logger?.LogInformation("Returning cached CW lookup data from SQL");
                return _cwCache;
            }

            _logger?.LogInformation("Fetching CW lookup data from SQL database");

            var result = new Dictionary<string, List<LookupItem>>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("dbo.usp_GetCWLookupData", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using var reader = await command.ExecuteReaderAsync();

            // Result Set 1: LineOfBusiness
            result["LineOfBusiness"] = await ReadLookupItemsAsync(reader);

            // Result Set 2: ProjectTheme
            await reader.NextResultAsync();
            result["ProjectTheme"] = await ReadLookupItemsAsync(reader);

            // Result Set 3: SWPRole
            await reader.NextResultAsync();
            result["SWPRole"] = await ReadLookupItemsAsync(reader);

            // Result Set 4: FundingType
            await reader.NextResultAsync();
            result["FundingType"] = await ReadLookupItemsAsync(reader);

            // Result Set 5: CountryList
            await reader.NextResultAsync();
            result["CountryList"] = await ReadLookupItemsAsync(reader);

            // Result Set 6: Status
            await reader.NextResultAsync();
            result["Status"] = await ReadLookupItemsAsync(reader);

            // Update cache
            _cwCache = result;
            _cwCacheTime = DateTime.UtcNow;

            _logger?.LogInformation("Successfully fetched and cached CW lookup data. Total categories: {Count}", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching CW lookup data from SQL database");
            throw;
        }
    }

    private async Task<Dictionary<string, List<LookupItem>>> GetFTELookupDataFromDatabaseAsync()
    {
        try
        {
            // Check cache
            if (_fteCache != null && _fteCacheTime.HasValue &&
                DateTime.UtcNow - _fteCacheTime.Value < _cacheExpiration)
            {
                _logger?.LogInformation("Returning cached FTE lookup data from SQL");
                return _fteCache;
            }

            _logger?.LogInformation("Fetching FTE lookup data from SQL database");

            var result = new Dictionary<string, List<LookupItem>>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("dbo.usp_GetFTELookupData", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using var reader = await command.ExecuteReaderAsync();

            // Result Set 1: Joblevel
            result["Joblevel"] = await ReadLookupItemsAsync(reader);

            // Result Set 2: SWPRole
            await reader.NextResultAsync();
            result["SWPRole"] = await ReadLookupItemsAsync(reader);

            // Result Set 3: PositionType
            await reader.NextResultAsync();
            result["PositionType"] = await ReadLookupItemsAsync(reader);

            // Result Set 4: FundingType
            await reader.NextResultAsync();
            result["FundingType"] = await ReadLookupItemsAsync(reader);

            // Result Set 5: RoleType
            await reader.NextResultAsync();
            result["RoleType"] = await ReadLookupItemsAsync(reader);

            // Result Set 6: CountryList
            await reader.NextResultAsync();
            result["CountryList"] = await ReadLookupItemsAsync(reader);

            // Result Set 7: Status
            await reader.NextResultAsync();
            result["Status"] = await ReadLookupItemsAsync(reader);

            // Result Set 8: StatusOfPosition
            await reader.NextResultAsync();
            result["StatusOfPosition"] = await ReadLookupItemsAsync(reader);

            // Update cache
            _fteCache = result;
            _fteCacheTime = DateTime.UtcNow;

            _logger?.LogInformation("Successfully fetched and cached FTE lookup data. Total categories: {Count}", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching FTE lookup data from SQL database");
            throw;
        }
    }

    private static async Task<List<LookupItem>> ReadLookupItemsAsync(SqlDataReader reader)
    {
        var items = new List<LookupItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new LookupItem
            {
                Value = reader["Value"].ToString() ?? string.Empty,
                Label = reader["Label"].ToString() ?? string.Empty
            });
        }

        return items;
    }

    // CW specific methods
    public async Task<List<LookupItem>> GetLineOfBusinessesAsync()
    {
        var data = await GetCWLookupDataFromDatabaseAsync();
        return data.TryGetValue("LineOfBusiness", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetProjectThemesAsync()
    {
        var data = await GetCWLookupDataFromDatabaseAsync();
        return data.TryGetValue("ProjectTheme", out var items) ? items : new List<LookupItem>();
    }

    // Shared methods - use FTE cache
    public async Task<List<LookupItem>> GetSWPRolesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("SWPRole", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetFundingTypesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("FundingType", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetCountryListAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("CountryList", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetPositionStatusesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("Status", out var items) ? items : new List<LookupItem>();
    }

    // FTE specific methods
    public async Task<List<LookupItem>> GetJobLevelsAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("Joblevel", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetPositionTypesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("PositionType", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetRoleTypesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("RoleType", out var items) ? items : new List<LookupItem>();
    }

    public async Task<List<LookupItem>> GetVacancyTypesAsync()
    {
        var data = await GetFTELookupDataFromDatabaseAsync();
        return data.TryGetValue("StatusOfPosition", out var items) ? items : new List<LookupItem>();
    }
}
