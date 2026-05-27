using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Approver Action operations using ADO.NET
/// </summary>
public class SqlApproverActionRepository : IApproverActionRepository
{
    private readonly string _connectionString;

    public SqlApproverActionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<ApproverActionResponse> ProcessApproverActionAsync(ApproverActionRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_ProcessApproverAction", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add parameters
            command.Parameters.AddWithValue("@RequestID", Guid.Parse(request.RequestID));
            command.Parameters.AddWithValue("@RequestorEmail", request.RequestorEmail);
            command.Parameters.AddWithValue("@ApproverName", request.ApproverName);
            command.Parameters.AddWithValue("@ApproverEmail", request.ApproverEmail);
            command.Parameters.AddWithValue("@ApproverAction", request.ApproverAction);
            command.Parameters.AddWithValue("@Comments", 
                string.IsNullOrEmpty(request.Comments) ? (object)DBNull.Value : request.Comments);
            command.Parameters.AddWithValue("@IsTesting", request.IsTesting);

            using var reader = await command.ExecuteReaderAsync();

            // Read result (stored procedure returns single row with status)
            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"] != DBNull.Value
                    ? reader["StatusCode"].ToString()
                    : "500";

                var message = reader["Message"] != DBNull.Value
                    ? reader["Message"].ToString()
                    : "Unknown error";

                // Check if this is an error response (has ErrorNumber column)
                var hasErrorNumber = false;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "ErrorNumber")
                    {
                        hasErrorNumber = true;
                        break;
                    }
                }

                if (hasErrorNumber || statusCode != "200")
                {
                    // This is an error response
                    return new ApproverActionResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                        Body = new ApproverActionResponseBody
                        {
                            Message = message ?? "Error processing approver action"
                        }
                    };
                }

                // Success response
                return new ApproverActionResponse
                {
                    StatusCode = statusCode ?? "200",
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = new ApproverActionResponseBody
                    {
                        Message = message ?? "Records Has updated and Notification sends to User",
                        RequestCode = reader["RequestCode"] != DBNull.Value
                            ? reader["RequestCode"].ToString()
                            : null,
                        Action = reader["Action"] != DBNull.Value
                            ? reader["Action"].ToString()
                            : null,
                        CurrentSeqOrder = reader["CurrentSeqOrder"] != DBNull.Value
                            ? Convert.ToInt32(reader["CurrentSeqOrder"])
                            : null,
                        MaxSeqOrder = reader["MaxSeqOrder"] != DBNull.Value
                            ? Convert.ToInt32(reader["MaxSeqOrder"])
                            : null,
                        ResultStatus = reader["ResultStatus"] != DBNull.Value
                            ? reader["ResultStatus"].ToString()
                            : null
                    }
                };
            }

            // No result returned
            return new ApproverActionResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = new ApproverActionResponseBody
                {
                    Message = "No result returned from stored procedure"
                }
            };
        }
        catch (SqlException ex)
        {
            return new ApproverActionResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = new ApproverActionResponseBody
                {
                    Message = $"Database error: {ex.Message}"
                }
            };
        }
        catch (Exception ex)
        {
            return new ApproverActionResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = new ApproverActionResponseBody
                {
                    Message = $"Error processing approver action: {ex.Message}"
                }
            };
        }
    }
}
