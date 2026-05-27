using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Position Distribution Status operations using ADO.NET
/// </summary>
public class SqlPositionDistributionStatusRepository : IPositionDistributionStatusRepository
{
    private readonly string _connectionString;

    public SqlPositionDistributionStatusRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<PositionDistributionStatusResponse> GetPositionDistributionStatusAsync(PositionDistributionStatusRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetPositionDistributionStatus", connection)
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

            var distributionList = new List<PositionDistributionItem>();

            while (await reader.ReadAsync())
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
                    return new PositionDistributionStatusResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new PositionDistributionStatusBody
                        {
                            Status = "error",
                            Data = new List<PositionDistributionItem>()
                        }
                    };
                }

                // Success response - read distribution data
                var period = reader["Period"] != DBNull.Value
                    ? reader["Period"].ToString()
                    : string.Empty;

                var type = reader["Type"] != DBNull.Value
                    ? reader["Type"].ToString()
                    : string.Empty;

                var count = reader["Count"] != DBNull.Value
                    ? Convert.ToInt32(reader["Count"])
                    : 0;

                var requestTpe = reader["RequestTpe"] != DBNull.Value
                    ? reader["RequestTpe"].ToString()
                    : string.Empty;

                distributionList.Add(new PositionDistributionItem
                {
                    Period = period ?? string.Empty,
                    Type = type ?? string.Empty,
                    Count = count,
                    RequestTpe = requestTpe ?? string.Empty
                });
            }

            return new PositionDistributionStatusResponse
            {
                StatusCode = "200",
                Body = new PositionDistributionStatusBody
                {
                    Status = "success",
                    Data = distributionList
                }
            };
        }
        catch (SqlException ex)
        {
            return new PositionDistributionStatusResponse
            {
                StatusCode = "500",
                Body = new PositionDistributionStatusBody
                {
                    Status = "error",
                    Data = new List<PositionDistributionItem>()
                }
            };
        }
        catch (Exception ex)
        {
            return new PositionDistributionStatusResponse
            {
                StatusCode = "500",
                Body = new PositionDistributionStatusBody
                {
                    Status = "error",
                    Data = new List<PositionDistributionItem>()
                }
            };
        }
    }
}
