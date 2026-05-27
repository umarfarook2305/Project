using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Position operations using ADO.NET
/// </summary>
public class SqlPositionRepository : IPositionRepository
{
    private readonly string _connectionString;

    public SqlPositionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<PositionResponse> GetPositionsAsync()
    {
        var response = new PositionResponse
        {
            Data = new List<PositionData>()
        };

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetPositionDetails", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120 // 2 minutes timeout for large datasets
            };

            using var reader = await command.ExecuteReaderAsync();

            // Read position data
            while (await reader.ReadAsync())
            {
                var position = new PositionData
                {
                    EmployeeName = reader["EmployeeName"] != DBNull.Value 
                        ? reader["EmployeeName"].ToString() 
                        : null,
                    EmployeeGuid = reader["EmployeeGuid"] != DBNull.Value 
                        ? reader["EmployeeGuid"].ToString() 
                        : null,
                    PositionId = reader["PositionId"] != DBNull.Value 
                        ? reader["PositionId"].ToString() 
                        : null,
                    PositionIdGuid = reader["PositionIdGuid"] != DBNull.Value 
                        ? reader["PositionIdGuid"].ToString() 
                        : null,
                    PositionStatus = reader["PositionStatus"] != DBNull.Value 
                        ? reader["PositionStatus"].ToString() 
                        : null,
                    JobRequisitionId = reader["JobRequisitionId"] != DBNull.Value 
                        ? reader["JobRequisitionId"].ToString() 
                        : null,
                    JobRequisitionGuid = reader["JobRequisitionGuid"] != DBNull.Value 
                        ? reader["JobRequisitionGuid"].ToString() 
                        : null,
                    JobProfileName = reader["JobProfileName"] != DBNull.Value 
                        ? reader["JobProfileName"].ToString() 
                        : null
                };

                response.Data.Add(position);
            }

            response.Count = response.Data.Count;
            response.Status = "success";
        }
        catch (Exception ex)
        {
            response.Status = "error";
            response.Count = 0;
            response.Data = new List<PositionData>();

            // Log the exception (in production, use proper logging)
            throw new Exception($"Error retrieving position data: {ex.Message}", ex);
        }

        return response;
    }
}
