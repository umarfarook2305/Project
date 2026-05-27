using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

public class SqlUpdateDelegateRepository : IUpdateDelegateRepository
{
    private readonly string _connectionString;

    public SqlUpdateDelegateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<UpdateDelegateResponse> UpdateDelegateAsync(UpdateDelegateRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_UpdateDelegate", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.AddWithValue("@DelegateId", Guid.Parse(request.DelegateId));
            command.Parameters.AddWithValue("@DelegateUserEmail", 
                string.IsNullOrWhiteSpace(request.DelegateUserEmail) ? DBNull.Value : request.DelegateUserEmail);
            command.Parameters.AddWithValue("@BehalfUserEmail", 
                string.IsNullOrWhiteSpace(request.BehalfUserEmail) ? DBNull.Value : request.BehalfUserEmail);

            command.Parameters.AddWithValue("@DelegateFrom", 
                string.IsNullOrWhiteSpace(request.DelegateFrom) || !DateTime.TryParse(request.DelegateFrom, out var from) 
                    ? DBNull.Value : from);

            command.Parameters.AddWithValue("@DelegateTo", 
                string.IsNullOrWhiteSpace(request.DelegateTo) || !DateTime.TryParse(request.DelegateTo, out var to) 
                    ? DBNull.Value : to);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"]?.ToString() ?? "500";
                var status = reader["Status"]?.ToString() ?? "error";
                var message = reader["Message"]?.ToString() ?? "Unknown error";
                var delegateId = reader["DelegateId"]?.ToString() ?? string.Empty;

                return new UpdateDelegateResponse
                {
                    StatusCode = statusCode,
                    Body = new UpdateDelegateBody
                    {
                        Status = status,
                        Message = message,
                        DelegateId = delegateId
                    }
                };
            }

            return new UpdateDelegateResponse
            {
                StatusCode = "500",
                Body = new UpdateDelegateBody
                {
                    Status = "error",
                    Message = "Failed to update delegation",
                    DelegateId = string.Empty
                }
            };
        }
        catch
        {
            return new UpdateDelegateResponse
            {
                StatusCode = "500",
                Body = new UpdateDelegateBody
                {
                    Status = "error",
                    Message = "Failed to update delegation",
                    DelegateId = string.Empty
                }
            };
        }
    }
}
