using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Job Family Distribution operations
/// </summary>
public class JobFamilyDistributionService : IJobFamilyDistributionService
{
    private readonly IJobFamilyDistributionRepository _repository;
    private static readonly List<string> ValidDurations = new() { "1M", "3M", "6M", "1Y", "FY" };
    private static readonly List<string> ValidFilterTypes = new() { "DURATION", "FINANCIAL_YEAR", "CUSTOM" };
    private static readonly List<string> ValidInsightsTypes = new() { "All", "FTE", "CW" };
    private static readonly List<string> ValidViewTypes = new() { "MyView", "MyTeam", "MyOrg", "All" };

    public JobFamilyDistributionService(IJobFamilyDistributionRepository repository)
    {
        _repository = repository;
    }

    public async Task<JobFamilyDistributionResponse> GetJobFamilyDistributionAsync(JobFamilyDistributionRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.GetJobFamilyDistributionAsync(request);
    }

    private static void ValidateRequest(JobFamilyDistributionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LoggedInEmail))
        {
            throw new ArgumentException("LoggedInEmail is required");
        }

        if (!string.IsNullOrWhiteSpace(request.FilterType) && !ValidFilterTypes.Contains(request.FilterType))
        {
            throw new ArgumentException($"FilterType must be one of: {string.Join(", ", ValidFilterTypes)}");
        }

        if (!string.IsNullOrWhiteSpace(request.InsightsType) && !ValidInsightsTypes.Contains(request.InsightsType))
        {
            throw new ArgumentException($"InsightsType must be one of: {string.Join(", ", ValidInsightsTypes)}");
        }

        if (!string.IsNullOrWhiteSpace(request.ViewType) && !ValidViewTypes.Contains(request.ViewType))
        {
            throw new ArgumentException($"ViewType must be one of: {string.Join(", ", ValidViewTypes)}");
        }

        // If CUSTOM filter type, both dates are required
        if (request.FilterType == "CUSTOM")
        {
            if (string.IsNullOrWhiteSpace(request.StartDate))
            {
                throw new ArgumentException("StartDate is required when FilterType is CUSTOM");
            }

            if (string.IsNullOrWhiteSpace(request.EndDate))
            {
                throw new ArgumentException("EndDate is required when FilterType is CUSTOM");
            }

            if (!DateTime.TryParse(request.StartDate, out _))
            {
                throw new ArgumentException("StartDate must be a valid date");
            }

            if (!DateTime.TryParse(request.EndDate, out _))
            {
                throw new ArgumentException("EndDate must be a valid date");
            }
        }

        // If DURATION filter type, duration can be provided
        if (request.FilterType == "DURATION" && !string.IsNullOrWhiteSpace(request.Duration))
        {
            if (!ValidDurations.Contains(request.Duration))
            {
                throw new ArgumentException($"Duration must be one of: {string.Join(", ", ValidDurations)}");
            }
        }
    }
}
