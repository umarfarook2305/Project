using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Approved Role Distribution operations using ADO.NET
/// </summary>
public class SqlApprovedRoleDistributionRepository : IApprovedRoleDistributionRepository
{
    private readonly string _connectionString;

    public SqlApprovedRoleDistributionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<ApprovedRoleDistributionResponse> GetApprovedRoleDistributionAsync(ApprovedRoleDistributionRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetApprovedRoleDistribution", connection)
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

            var roleTypeList = new List<RoleTypeCount>();
            int totalCount = 0;

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
                    return new ApprovedRoleDistributionResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new ApprovedRoleDistributionBody
                        {
                            Status = "error",
                            TotalCount = 0,
                            Data = new List<RoleTypeCount>()
                        }
                    };
                }

                // Get total count (same for all rows)
                if (totalCount == 0 && reader["TotalCount"] != DBNull.Value)
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                // Success response - read role type data
                var roleType = reader["RoleType"] != DBNull.Value
                    ? reader["RoleType"].ToString()
                    : string.Empty;

                var count = reader["Count"] != DBNull.Value
                    ? Convert.ToInt32(reader["Count"])
                    : 0;

                if (!string.IsNullOrEmpty(roleType))
                {
                    roleTypeList.Add(new RoleTypeCount
                    {
                        RoleType = roleType ?? string.Empty,
                        Count = count
                    });
                }
            }

            return new ApprovedRoleDistributionResponse
            {
                StatusCode = "200",
                Body = new ApprovedRoleDistributionBody
                {
                    Status = "success",
                    TotalCount = totalCount,
                    Data = roleTypeList
                }
            };
        }
        catch (SqlException ex)
        {
            return new ApprovedRoleDistributionResponse
            {
                StatusCode = "500",
                Body = new ApprovedRoleDistributionBody
                {
                    Status = "error",
                    TotalCount = 0,
                    Data = new List<RoleTypeCount>()
                }
            };
        }
        catch (Exception ex)
        {
            return new ApprovedRoleDistributionResponse
            {
                StatusCode = "500",
                Body = new ApprovedRoleDistributionBody
                {
                    Status = "error",
                    TotalCount = 0,
                    Data = new List<RoleTypeCount>()
                }
            };
        }
    }
}
