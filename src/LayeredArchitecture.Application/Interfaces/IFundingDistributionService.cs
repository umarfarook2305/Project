using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Service interface for Funding Distribution operations
/// </summary>
public interface IFundingDistributionService
{
    /// <summary>
    /// Get funding distribution statistics (Project Funded vs Base Funded)
    /// </summary>
    Task<FundingDistributionResponse> GetFundingDistributionAsync(FundingDistributionRequest request);
}
