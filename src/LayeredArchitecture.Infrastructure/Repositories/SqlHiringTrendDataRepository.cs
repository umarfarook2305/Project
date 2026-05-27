using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Hiring Trend Data operations using ADO.NET
/// </summary>
public class SqlHiringTrendDataRepository : IHiringTrendDataRepository
{
    private readonly string _connectionString;

    public SqlHiringTrendDataRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<HiringTrendDataResponse> GetHiringTrendDataAsync(HiringTrendDataRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetHiringTrendData", connection)
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

            command.Parameters.AddWithValue("@LoggedInEmail", request.LoggedInEmail);
            command.Parameters.AddWithValue("@ViewType", 
                string.IsNullOrEmpty(request.ViewType) ? "MyView" : request.ViewType);

            using var reader = await command.ExecuteReaderAsync();

            var trendDataList = new List<HiringTrendDataPoint>();

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
                    return new HiringTrendDataResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new HiringTrendDataBody
                        {
                            Status = "error",
                            Data = new List<HiringTrendDataPoint>()
                        }
                    };
                }

                // Success response - read trend data
                var date = reader["Date"] != DBNull.Value
                    ? reader["Date"].ToString()
                    : string.Empty;

                var fte = reader["FTE"] != DBNull.Value
                    ? reader["FTE"].ToString()
                    : "0";

                var cw = reader["CW"] != DBNull.Value
                    ? reader["CW"].ToString()
                    : "0";

                trendDataList.Add(new HiringTrendDataPoint
                {
                    Date = date ?? string.Empty,
                    FTE = fte ?? "0",
                    CW = cw ?? "0"
                });
            }

            return new HiringTrendDataResponse
            {
                StatusCode = "200",
                Body = new HiringTrendDataBody
                {
                    Status = "success",
                    Data = trendDataList
                }
            };
        }
        catch (SqlException ex)
        {
            return new HiringTrendDataResponse
            {
                StatusCode = "500",
                Body = new HiringTrendDataBody
                {
                    Status = "error",
                    Data = new List<HiringTrendDataPoint>()
                }
            };
        }
        catch (Exception ex)
        {
            return new HiringTrendDataResponse
            {
                StatusCode = "500",
                Body = new HiringTrendDataBody
                {
                    Status = "error",
                    Data = new List<HiringTrendDataPoint>()
                }
            };
        }
    }
}
