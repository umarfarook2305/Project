using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Hiring Trend Data operations
/// </summary>
public class HiringTrendDataService : IHiringTrendDataService
{
    private readonly IHiringTrendDataRepository _repository;
    private static readonly List<string> ValidDurations = new() { "1M", "3M", "6M", "1Y", "FY" };
    private static readonly List<string> ValidFilterTypes = new() { "DURATION", "FINANCIAL_YEAR", "CUSTOM" };
    private static readonly List<string> ValidViewTypes = new() { "MyView", "MyTeam", "MyOrg", "All" };

    public HiringTrendDataService(IHiringTrendDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<HiringTrendDataResponse> GetHiringTrendDataAsync(HiringTrendDataRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.GetHiringTrendDataAsync(request);
    }

    private static void ValidateRequest(HiringTrendDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LoggedInEmail))
        {
            throw new ArgumentException("LoggedInEmail is required");
        }

        if (!string.IsNullOrWhiteSpace(request.FilterType) && !ValidFilterTypes.Contains(request.FilterType))
        {
            throw new ArgumentException($"FilterType must be one of: {string.Join(", ", ValidFilterTypes)}");
        }

        if (!string.IsNullOrWhiteSpace(request.ViewType) && !ValidViewTypes.Contains(request.ViewType))
        {
            throw new ArgumentException($"ViewType must be one of: {string.Join(", ", ValidViewTypes)}");
        }

        // If CUSTOM or FINANCIAL_YEAR filter type, both dates are required
        if (request.FilterType == "CUSTOM" || request.FilterType == "FINANCIAL_YEAR")
        {
            if (string.IsNullOrWhiteSpace(request.StartDate))
            {
                throw new ArgumentException("StartDate is required when FilterType is CUSTOM or FINANCIAL_YEAR");
            }

            if (string.IsNullOrWhiteSpace(request.EndDate))
            {
                throw new ArgumentException("EndDate is required when FilterType is CUSTOM or FINANCIAL_YEAR");
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
