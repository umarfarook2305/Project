using HART.Domain.Entities;
using HART.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace HART.Infrastructure.Repositories;

public class SqlJobProfileRepository : IJobProfileRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlJobProfileRepository>? _logger;

    // In-memory cache
    private List<JobProfile>? _allProfilesCache;
    private Dictionary<string, List<JobProfile>>? _filteredCache;
    private DateTime? _cacheTime;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);
    private readonly object _cacheLock = new();

    public SqlJobProfileRepository(
        IConfiguration configuration,
        ILogger<SqlJobProfileRepository>? logger = null)
    {
        _connectionString = configuration.GetConnectionString("HARTDatabase")
            ?? throw new InvalidOperationException("Connection string 'HARTDatabase' not found.");
        _logger = logger;
        _filteredCache = new Dictionary<string, List<JobProfile>>();
    }

    public async Task<List<JobProfile>> GetJobProfilesAsync(string? jobLevel = null)
    {
        try
        {
            var cacheKey = jobLevel ?? "ALL";

            // Check cache
            lock (_cacheLock)
            {
                if (_cacheTime.HasValue && DateTime.UtcNow - _cacheTime.Value < _cacheExpiration)
                {
                    if (string.IsNullOrWhiteSpace(jobLevel) && _allProfilesCache != null)
                    {
                        _logger?.LogInformation("Returning cached job profiles (ALL)");
                        return _allProfilesCache;
                    }

                    if (_filteredCache != null && _filteredCache.ContainsKey(cacheKey))
                    {
                        _logger?.LogInformation("Returning cached job profiles for: {JobLevel}", cacheKey);
                        return _filteredCache[cacheKey];
                    }
                }
            }

            _logger?.LogInformation("Fetching job profiles from SQL database with filter: {JobLevel}", jobLevel ?? "ALL");

            var profiles = new List<JobProfile>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("dbo.usp_GetJobProfiles", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            // Add optional parameter
            command.Parameters.Add(new SqlParameter("@JobLevel", SqlDbType.NVarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(jobLevel) ? DBNull.Value : jobLevel
            });

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                profiles.Add(new JobProfile
                {
                    ProfileId = reader["ProfileID"]?.ToString() ?? string.Empty,
                    ProfileName = reader["Profile"]?.ToString() ?? string.Empty,
                    FamilyId = reader["FamilyID"] == DBNull.Value ? null : reader["FamilyID"]?.ToString(),
                    FamilyName = reader["Family"] == DBNull.Value ? null : reader["Family"]?.ToString()
                });
            }

            // Update cache
            lock (_cacheLock)
            {
                if (string.IsNullOrWhiteSpace(jobLevel))
                {
                    _allProfilesCache = profiles;
                }
                else
                {
                    _filteredCache ??= new Dictionary<string, List<JobProfile>>();
                    _filteredCache[cacheKey] = profiles;
                }

                _cacheTime = DateTime.UtcNow;
            }

            _logger?.LogInformation("Successfully fetched {Count} job profiles", profiles.Count);

            return profiles;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching job profiles from SQL database");
            throw;
        }
    }
}
