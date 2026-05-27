using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

public class SqlInsertDelegateRepository : IInsertDelegateRepository
{
    private readonly string _connectionString;

    public SqlInsertDelegateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<InsertDelegateResponse> InsertDelegateAsync(InsertDelegateRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_InsertDelegate", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            // Add parameters (only email and date columns exist in table)
            command.Parameters.AddWithValue("@DelegateUserEmail", request.DelegateUserEmail);
            command.Parameters.AddWithValue("@BehalfUserEmail", request.BehalfUserEmail);

            if (DateTime.TryParse(request.DelegateFrom, out var fromDate))
                command.Parameters.AddWithValue("@DelegateFrom", fromDate);
            else
                command.Parameters.AddWithValue("@DelegateFrom", DBNull.Value);

            if (DateTime.TryParse(request.DelegateTo, out var toDate))
                command.Parameters.AddWithValue("@DelegateTo", toDate);
            else
                command.Parameters.AddWithValue("@DelegateTo", DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"]?.ToString() ?? "500";
                var status = reader["Status"]?.ToString() ?? "error";
                var message = reader["Message"]?.ToString() ?? "Unknown error";
                var delegateId = reader.GetOrdinal("DelegateId") >= 0 && !reader.IsDBNull(reader.GetOrdinal("DelegateId"))
                    ? reader["DelegateId"]?.ToString()
                    : null;
                var delegateCode = reader.GetOrdinal("DelegateCode") >= 0 && !reader.IsDBNull(reader.GetOrdinal("DelegateCode"))
                    ? reader["DelegateCode"]?.ToString()
                    : null;

                return new InsertDelegateResponse
                {
                    StatusCode = statusCode,
                    Body = new InsertDelegateBody
                    {
                        Status = status,
                        Message = message,
                        DelegateId = delegateId,
                        DelegateCode = delegateCode
                    }
                };
            }

            return new InsertDelegateResponse
            {
                StatusCode = "500",
                Body = new InsertDelegateBody
                {
                    Status = "error",
                    Message = "Failed to create delegation"
                }
            };
        }
        catch
        {
            return new InsertDelegateResponse
            {
                StatusCode = "500",
                Body = new InsertDelegateBody
                {
                    Status = "error",
                    Message = "Failed to create delegation"
                }
            };
        }
    }
}
