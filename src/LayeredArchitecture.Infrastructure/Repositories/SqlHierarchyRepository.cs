using HART.Domain.Entities;
using HART.Domain.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HART.Infrastructure.Repositories;

public class SqlHierarchyRepository : IHierarchyRepository
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<SqlHierarchyRepository>? _logger;

    // In-memory cache with 30-minute expiration
    private Dictionary<string, (List<EmployeeHierarchy> Data, DateTime CacheTime)>? _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);
    private readonly object _cacheLock = new();

    public SqlHierarchyRepository(
        IConfiguration configuration,
        ILogger<SqlHierarchyRepository>? logger = null)
    {
        _logger = logger;
        _cache = new Dictionary<string, (List<EmployeeHierarchy>, DateTime)>();

        // Initialize Graph Client with credentials from configuration
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException(
                "Azure AD configuration is missing. Please configure AzureAd:TenantId, AzureAd:ClientId, and AzureAd:ClientSecret in appsettings.json");
        }

        var clientSecretCredential = new ClientSecretCredential(
            tenantId,
            clientId,
            clientSecret);

        _graphClient = new GraphServiceClient(clientSecretCredential);
    }

    public async Task<List<EmployeeHierarchy>> GetEmployeeHierarchyAsync(string email)
    {
        try
        {
            var cacheKey = email.ToLowerInvariant();

            // Check cache
            lock (_cacheLock)
            {
                if (_cache != null && _cache.ContainsKey(cacheKey))
                {
                    var (cachedData, cacheTime) = _cache[cacheKey];
                    if (DateTime.UtcNow - cacheTime < _cacheExpiration)
                    {
                        _logger?.LogInformation("Returning cached hierarchy from Graph API for: {Email}", email);
                        return cachedData;
                    }
                }
            }

            _logger?.LogInformation("Fetching hierarchy from Microsoft Graph API for: {Email}", email);

            var hierarchy = new List<EmployeeHierarchy>();
            var currentEmail = email;
            var level = 1;
            var maxLevels = 10; // Safety limit to prevent infinite loops

            // Iteratively fetch managers up the chain
            while (!string.IsNullOrEmpty(currentEmail) && level <= maxLevels)
            {
                try
                {
                    // Get user by email
                    var user = await _graphClient.Users[currentEmail].GetAsync();

                    if (user == null)
                    {
                        _logger?.LogWarning("User not found: {Email}", currentEmail);
                        break;
                    }

                    // Get user's manager
                    var manager = await _graphClient.Users[user.Id].Manager.GetAsync();

                    if (manager == null)
                    {
                        _logger?.LogInformation("No manager found for: {Email}. Reached top of hierarchy.", currentEmail);
                        break;
                    }

                    // Cast manager to User type
                    if (manager is User managerUser)
                    {
                        hierarchy.Add(new EmployeeHierarchy
                        {
                            Level = level,
                            Name = managerUser.DisplayName ?? string.Empty,
                            Email = managerUser.Mail ?? managerUser.UserPrincipalName ?? string.Empty,
                            Title = managerUser.JobTitle ?? string.Empty
                        });

                        // Move up to the next manager
                        currentEmail = managerUser.Mail ?? managerUser.UserPrincipalName;
                        level++;
                    }
                    else
                    {
                        _logger?.LogWarning("Manager is not a User type for: {Email}", currentEmail);
                        break;
                    }
                }
                catch (ServiceException ex) when (ex.ResponseStatusCode == 404)
                {
                    _logger?.LogWarning("User or manager not found: {Email}", currentEmail);
                    break;
                }
                catch (ServiceException ex) when (ex.ResponseStatusCode == 403)
                {
                    _logger?.LogError(ex, "Insufficient permissions to access Graph API for: {Email}", currentEmail);
                    throw new UnauthorizedAccessException(
                        "Insufficient permissions to access Microsoft Graph API. Please ensure the application has User.Read.All or Directory.Read.All permissions.", ex);
                }
            }

            if (level > maxLevels)
            {
                _logger?.LogWarning("Maximum hierarchy depth ({MaxLevels}) reached for: {Email}", maxLevels, email);
            }

            // Update cache
            lock (_cacheLock)
            {
                _cache ??= new Dictionary<string, (List<EmployeeHierarchy>, DateTime)>();
                _cache[cacheKey] = (hierarchy, DateTime.UtcNow);
            }

            _logger?.LogInformation("Successfully fetched {Count} hierarchy levels from Graph API for: {Email}",
                hierarchy.Count, email);

            return hierarchy;
        }
        catch (ServiceException ex)
        {
            _logger?.LogError(ex, "Microsoft Graph API error for email: {Email}. Status: {StatusCode}, Message: {Message}",
                email, ex.ResponseStatusCode, ex.Message);
            throw new InvalidOperationException($"Failed to fetch hierarchy from Microsoft Graph API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching hierarchy from Graph API for email: {Email}", email);
            throw;
        }
    }
}
