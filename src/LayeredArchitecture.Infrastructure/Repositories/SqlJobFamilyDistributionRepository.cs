using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Job Family Distribution operations using ADO.NET
/// </summary>
public class SqlJobFamilyDistributionRepository : IJobFamilyDistributionRepository
{
    private readonly string _connectionString;

    public SqlJobFamilyDistributionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<JobFamilyDistributionResponse> GetJobFamilyDistributionAsync(JobFamilyDistributionRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetJobFamilyDistribution", connection)
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

            var jobFamilyList = new List<JobFamilyCount>();

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
                    return new JobFamilyDistributionResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new JobFamilyDistributionBody
                        {
                            Status = "error",
                            Data = new List<JobFamilyCount>()
                        }
                    };
                }

                // Success response - read job family data
                var jobFamily = reader["JobFamily"] != DBNull.Value
                    ? reader["JobFamily"].ToString()
                    : string.Empty;

                var count = reader["Count"] != DBNull.Value
                    ? Convert.ToInt32(reader["Count"])
                    : 0;

                if (!string.IsNullOrEmpty(jobFamily))
                {
                    jobFamilyList.Add(new JobFamilyCount
                    {
                        JobFamily = jobFamily ?? string.Empty,
                        Count = count
                    });
                }
            }

            return new JobFamilyDistributionResponse
            {
                StatusCode = "200",
                Body = new JobFamilyDistributionBody
                {
                    Status = "success",
                    Data = jobFamilyList
                }
            };
        }
        catch (SqlException ex)
        {
            return new JobFamilyDistributionResponse
            {
                StatusCode = "500",
                Body = new JobFamilyDistributionBody
                {
                    Status = "error",
                    Data = new List<JobFamilyCount>()
                }
            };
        }
        catch (Exception ex)
        {
            return new JobFamilyDistributionResponse
            {
                StatusCode = "500",
                Body = new JobFamilyDistributionBody
                {
                    Status = "error",
                    Data = new List<JobFamilyCount>()
                }
            };
        }
    }
}
