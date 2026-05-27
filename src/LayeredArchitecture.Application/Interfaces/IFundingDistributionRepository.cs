using HART.Application.DTOs;

namespace HART.Application.Interfaces;

/// <summary>
/// Repository interface for Funding Distribution operations
/// </summary>
public interface IFundingDistributionRepository
{
    /// <summary>
    /// Get funding distribution statistics from database
    /// </summary>
    Task<FundingDistributionResponse> GetFundingDistributionAsync(FundingDistributionRequest request);
}
