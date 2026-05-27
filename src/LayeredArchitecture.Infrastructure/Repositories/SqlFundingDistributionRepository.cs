using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Funding Distribution operations using ADO.NET
/// </summary>
public class SqlFundingDistributionRepository : IFundingDistributionRepository
{
    private readonly string _connectionString;

    public SqlFundingDistributionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<FundingDistributionResponse> GetFundingDistributionAsync(FundingDistributionRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetFundingDistribution", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };

            // Add parameters
            command.Parameters.AddWithValue("@Duration", 
                string.IsNullOrEmpty(request.Duration) ? (object)DBNull.Value : request.Duration);
            command.Parameters.AddWithValue("@FilterType", 
                string.IsNullOrEmpty(request.FilterType) ? "DURATION" : request.FilterType);

            // Handle date parameters
            if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out var startDate))
            {
                command.Parameters.AddWithValue("@StartDate", startDate);
            }
            else
            {
                command.Parameters.AddWithValue("@StartDate", DBNull.Value);
            }

            if (!string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out var endDate))
            {
                command.Parameters.AddWithValue("@EndDate", endDate);
            }
            else
            {
                command.Parameters.AddWithValue("@EndDate", DBNull.Value);
            }

            command.Parameters.AddWithValue("@InsightsType", 
                string.IsNullOrEmpty(request.InsightsType) ? "All" : request.InsightsType);
            command.Parameters.AddWithValue("@LoggedInEmail", request.LoggedInEmail);
            command.Parameters.AddWithValue("@ViewType", 
                string.IsNullOrEmpty(request.ViewType) ? "MyView" : request.ViewType);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"] != DBNull.Value
                    ? reader["StatusCode"].ToString()
                    : "500";

                var status = reader["Status"] != DBNull.Value
                    ? reader["Status"].ToString()
                    : "error";

                // Check if this is an error response
                if (statusCode != "200" || status != "success")
                {
                    return new FundingDistributionResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new FundingDistributionBody
                        {
                            Status = "error",
                            Data = new FundingDistributionData()
                        }
                    };
                }

                // Success response - read statistics
                var total = reader["Total"] != DBNull.Value
                    ? Convert.ToInt32(reader["Total"])
                    : 0;

                var projectFundedTotal = reader["ProjectFundedTotal"] != DBNull.Value
                    ? Convert.ToInt32(reader["ProjectFundedTotal"])
                    : 0;

                var projectFundedFTE = reader["ProjectFundedFTE"] != DBNull.Value
                    ? Convert.ToInt32(reader["ProjectFundedFTE"])
                    : 0;

                var projectFundedCW = reader["ProjectFundedCW"] != DBNull.Value
                    ? Convert.ToInt32(reader["ProjectFundedCW"])
                    : 0;

                var baseFundedTotal = reader["BaseFundedTotal"] != DBNull.Value
                    ? Convert.ToInt32(reader["BaseFundedTotal"])
                    : 0;

                var baseFundedFTE = reader["BaseFundedFTE"] != DBNull.Value
                    ? Convert.ToInt32(reader["BaseFundedFTE"])
                    : 0;

                var baseFundedCW = reader["BaseFundedCW"] != DBNull.Value
                    ? Convert.ToInt32(reader["BaseFundedCW"])
                    : 0;

                var calculatedStartDate = reader["StartDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd")
                    : null;

                var calculatedEndDate = reader["EndDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd")
                    : null;

                return new FundingDistributionResponse
                {
                    StatusCode = "200",
                    Body = new FundingDistributionBody
                    {
                        Status = "success",
                        Data = new FundingDistributionData
                        {
                            Total = total.ToString(),
                            ProjectFunded = new FundingTypeDetail
                            {
                                Total = projectFundedTotal.ToString(),
                                FTE = projectFundedFTE.ToString(),
                                CW = projectFundedCW.ToString()
                            },
                            BaseFunded = new FundingTypeDetail
                            {
                                Total = baseFundedTotal.ToString(),
                                FTE = baseFundedFTE.ToString(),
                                CW = baseFundedCW.ToString()
                            },
                            StartDate = calculatedStartDate,
                            EndDate = calculatedEndDate
                        }
                    }
                };
            }

            // No result returned
            return new FundingDistributionResponse
            {
                StatusCode = "500",
                Body = new FundingDistributionBody
                {
                    Status = "error",
                    Data = new FundingDistributionData()
                }
            };
        }
        catch (SqlException ex)
        {
            return new FundingDistributionResponse
            {
                StatusCode = "500",
                Body = new FundingDistributionBody
                {
                    Status = "error",
                    Data = new FundingDistributionData()
                }
            };
        }
        catch (Exception ex)
        {
            return new FundingDistributionResponse
            {
                StatusCode = "500",
                Body = new FundingDistributionBody
                {
                    Status = "error",
                    Data = new FundingDistributionData()
                }
            };
        }
    }
}
