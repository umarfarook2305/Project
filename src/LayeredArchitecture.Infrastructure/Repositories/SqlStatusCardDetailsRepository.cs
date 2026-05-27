using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Status Card Details operations using ADO.NET
/// </summary>
public class SqlStatusCardDetailsRepository : IStatusCardDetailsRepository
{
    private readonly string _connectionString;

    public SqlStatusCardDetailsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<StatusCardDetailsResponse> GetStatusCardDetailsAsync(StatusCardDetailsRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetStatusCardDetails", connection)
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
                    var errorMessage = reader["ErrorMessage"] != DBNull.Value
                        ? reader["ErrorMessage"].ToString()
                        : "Unknown error occurred";

                    return new StatusCardDetailsResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new StatusCardBody
                        {
                            Status = "error",
                            Data = new StatusCardData()
                        }
                    };
                }

                // Success response - read statistics
                var total = reader["Total"] != DBNull.Value
                    ? Convert.ToInt32(reader["Total"])
                    : 0;

                var fte = reader["FTE"] != DBNull.Value
                    ? Convert.ToInt32(reader["FTE"])
                    : 0;

                var cw = reader["CW"] != DBNull.Value
                    ? Convert.ToInt32(reader["CW"])
                    : 0;

                var approved = reader["Approved"] != DBNull.Value
                    ? Convert.ToInt32(reader["Approved"])
                    : 0;

                var pending = reader["Pending"] != DBNull.Value
                    ? Convert.ToInt32(reader["Pending"])
                    : 0;

                var rejected = reader["Rejected"] != DBNull.Value
                    ? Convert.ToInt32(reader["Rejected"])
                    : 0;

                var calculatedStartDate = reader["StartDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd")
                    : null;

                var calculatedEndDate = reader["EndDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd")
                    : null;

                return new StatusCardDetailsResponse
                {
                    StatusCode = "200",
                    Body = new StatusCardBody
                    {
                        Status = "success",
                        Data = new StatusCardData
                        {
                            Total = total,
                            FTE = fte,
                            CW = cw,
                            Approved = approved,
                            Pending = pending,
                            Rejected = rejected,
                            StartDate = calculatedStartDate,
                            EndDate = calculatedEndDate
                        }
                    }
                };
            }

            // No result returned
            return new StatusCardDetailsResponse
            {
                StatusCode = "500",
                Body = new StatusCardBody
                {
                    Status = "error",
                    Data = new StatusCardData()
                }
            };
        }
        catch (SqlException ex)
        {
            return new StatusCardDetailsResponse
            {
                StatusCode = "500",
                Body = new StatusCardBody
                {
                    Status = "error",
                    Data = new StatusCardData()
                }
            };
        }
        catch (Exception ex)
        {
            return new StatusCardDetailsResponse
            {
                StatusCode = "500",
                Body = new StatusCardBody
                {
                    Status = "error",
                    Data = new StatusCardData()
                }
            };
        }
    }
}
