using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Cancel Delegate operations using ADO.NET
/// </summary>
public class SqlCancelDelegateRepository : ICancelDelegateRepository
{
    private readonly string _connectionString;

    public SqlCancelDelegateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<CancelDelegateResponse> CancelDelegateAsync(CancelDelegateRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_CancelDelegate", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            // Add parameters
            command.Parameters.AddWithValue("@DelegateId", Guid.Parse(request.DelegateID));

            if (DateTime.TryParse(request.CancelledOn, out var cancelledOn))
            {
                command.Parameters.AddWithValue("@CancelledOn", cancelledOn.Date);
            }
            else
            {
                command.Parameters.AddWithValue("@CancelledOn", DateTime.UtcNow.Date);
            }

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"] != DBNull.Value
                    ? reader["StatusCode"].ToString()
                    : "500";

                var status = reader["Status"] != DBNull.Value
                    ? reader["Status"].ToString()
                    : "error";

                var message = reader["Message"] != DBNull.Value
                    ? reader["Message"].ToString()
                    : "Unknown error";

                // Check if this is an error response (404 or 500)
                if (statusCode != "200" || status != "success")
                {
                    return new CancelDelegateResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                        Body = new CancelDelegateBody
                        {
                            Status = statusCode ?? "500",
                            Message = message ?? "Error while Cancelling Delegate",
                            Data = null
                        }
                    };
                }

                // Success response
                var delegatedBy = reader["DelegatedBy"] != DBNull.Value
                    ? reader["DelegatedBy"].ToString()
                    : string.Empty;

                var cancelledOnValue = reader["CancelledOn"] != DBNull.Value
                    ? Convert.ToDateTime(reader["CancelledOn"])
                    : DateTime.MinValue;

                return new CancelDelegateResponse
                {
                    StatusCode = "200",
                    Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                    Body = new CancelDelegateBody
                    {
                        Status = "200",
                        Message = "Delegate Cancelled Successfully",
                        Data = new CancelDelegateData
                        {
                            DelegatedBy = delegatedBy ?? string.Empty,
                            CancelledOn = cancelledOnValue.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        }
                    }
                };
            }

            // No result returned
            return new CancelDelegateResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                Body = new CancelDelegateBody
                {
                    Status = "500",
                    Message = "Error while Cancelling Delegate",
                    Data = null
                }
            };
        }
        catch (SqlException ex)
        {
            return new CancelDelegateResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                Body = new CancelDelegateBody
                {
                    Status = "500",
                    Message = "Error while Cancelling Delegate",
                    Data = null
                }
            };
        }
        catch (Exception ex)
        {
            return new CancelDelegateResponse
            {
                StatusCode = "500",
                Headers = new Dictionary<string, string> { { "content-type", "application/json" } },
                Body = new CancelDelegateBody
                {
                    Status = "500",
                    Message = "Error while Cancelling Delegate",
                    Data = null
                }
            };
        }
    }
}
