using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Approval List operations using ADO.NET
/// </summary>
public class SqlApprovalListRepository : IApprovalListRepository
{
    private readonly string _connectionString;

    public SqlApprovalListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<ApprovalListResponse> GetApprovalListAsync(ApprovalListRequest request)
    {
        var items = new List<ApprovalListItem>();
        int totalRecords = 0;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetApprovalList", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add required parameters
            command.Parameters.AddWithValue("@ApproverEmails", string.Join(",", request.Approver));
            command.Parameters.AddWithValue("@IsPending", request.IsPending);
            command.Parameters.AddWithValue("@Count", request.Count);

            // Add pagination parameters
            command.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            command.Parameters.AddWithValue("@PageSize", request.PageSize);

            // Add optional filter parameters
            command.Parameters.AddWithValue("@LoggedInEmail", 
                string.IsNullOrEmpty(request.LoggedInEmail) ? (object)DBNull.Value : request.LoggedInEmail);
            command.Parameters.AddWithValue("@ViewType", 
                string.IsNullOrEmpty(request.ViewType) ? (object)DBNull.Value : request.ViewType);
            command.Parameters.AddWithValue("@PendingWith", 
                string.IsNullOrEmpty(request.PendingWith) ? (object)DBNull.Value : request.PendingWith);
            command.Parameters.AddWithValue("@PositionType", 
                string.IsNullOrEmpty(request.PositionType) ? (object)DBNull.Value : request.PositionType);
            command.Parameters.AddWithValue("@RequestType", 
                string.IsNullOrEmpty(request.Type) ? (object)DBNull.Value : request.Type);
            command.Parameters.AddWithValue("@Funding", 
                string.IsNullOrEmpty(request.Funding) ? (object)DBNull.Value : request.Funding);
            command.Parameters.AddWithValue("@Level", 
                string.IsNullOrEmpty(request.Level) ? (object)DBNull.Value : request.Level);
            command.Parameters.AddWithValue("@Requestors", 
                request.Requestor != null && request.Requestor.Count > 0 
                    ? string.Join(",", request.Requestor) 
                    : (object)DBNull.Value);

            // Add timeline parameters
            if (request.Timeline != null)
            {
                command.Parameters.AddWithValue("@TimelineRangeType", request.Timeline.RangeType ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TimelineMonths", request.Timeline.Value ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TimelineStartDate", request.Timeline.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TimelineEndDate", request.Timeline.EndDate ?? (object)DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@TimelineRangeType", DBNull.Value);
                command.Parameters.AddWithValue("@TimelineMonths", DBNull.Value);
                command.Parameters.AddWithValue("@TimelineStartDate", DBNull.Value);
                command.Parameters.AddWithValue("@TimelineEndDate", DBNull.Value);
            }

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Read TotalRecords from first row
                if (totalRecords == 0 && reader["TotalRecords"] != DBNull.Value)
                {
                    totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }

                var item = new ApprovalListItem
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
                    PendingWith = reader["PendingWith"] != DBNull.Value
                        ? reader["PendingWith"].ToString()
                        : null,
                    Position = reader["Position"] != DBNull.Value
                        ? reader["Position"].ToString()
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
                    ApproverName = reader["ApproverName"] != DBNull.Value
                        ? reader["ApproverName"].ToString()
                        : null
                };

                items.Add(item);
            }

            // Calculate pagination metadata
            int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new ApprovalListResponse
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
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving approval list: {ex.Message}", ex);
        }
    }
}
