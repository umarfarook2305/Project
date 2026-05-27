using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Approval Delegation List operations using ADO.NET
/// </summary>
public class SqlApprovalDelegationListRepository : IApprovalDelegationListRepository
{
    private readonly string _connectionString;

    public SqlApprovalDelegationListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<ApprovalDelegationListResponse> GetApprovalDelegationListAsync(ApprovalDelegationListRequest request)
    {
        var items = new List<ApprovalDelegationListItem>();
        int totalRecords = 0;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetApprovalDelegationList", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add parameters
            command.Parameters.AddWithValue("@CurrentUserEmail", request.CurrentUserEmail);
            command.Parameters.AddWithValue("@IsPending", request.IsPending);
            command.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            command.Parameters.AddWithValue("@PageSize", request.PageSize);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Read TotalRecords from first row
                if (totalRecords == 0 && reader["TotalRecords"] != DBNull.Value)
                {
                    totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }

                var item = new ApprovalDelegationListItem
                {
                    RequestUniqueId = reader["RequestUniqueId"] != DBNull.Value
                        ? (Guid?)reader["RequestUniqueId"]
                        : null,
                    RequestId = reader["RequestId"] != DBNull.Value
                        ? reader["RequestId"].ToString()
                        : null,
                    Status = reader["Status"] != DBNull.Value
                        ? reader["Status"].ToString()
                        : null,
                    Requestor = reader["Requestor"] != DBNull.Value
                        ? reader["Requestor"].ToString()
                        : null,
                    Funding = reader["Funding"] != DBNull.Value
                        ? reader["Funding"].ToString()
                        : null,
                    Type = reader["Type"] != DBNull.Value
                        ? reader["Type"].ToString()
                        : null,
                    Level = reader["Level"] != DBNull.Value
                        ? reader["Level"].ToString()
                        : null,
                    SubmittedOn = reader["SubmittedOn"] != DBNull.Value
                        ? (DateTime?)reader["SubmittedOn"]
                        : null,
                    CompletedOn = reader["CompletedOn"] != DBNull.Value
                        ? (DateTime?)reader["CompletedOn"]
                        : null,
                    Delegator = reader["Delegator"] != DBNull.Value
                        ? reader["Delegator"].ToString()
                        : null
                };

                items.Add(item);
            }

            // Calculate pagination metadata
            int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new ApprovalDelegationListResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = items,
                Pagination = new PaginationMetadata
                {
                    TotalRecords = totalRecords,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                }
            };
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException($"Database error getting delegated approval list: {ex.Message}", ex);
        }
    }
}
