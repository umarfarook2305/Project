using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

public class SqlRequestFilterRepository : IRequestFilterRepository
{
    private readonly string _connectionString;

    public SqlRequestFilterRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<PaginatedResponse<RequestListItemResponse>> GetRequestListAsync(RequestFilterRequest filter)
    {
        var response = new PaginatedResponse<RequestListItemResponse>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_GetRequestDetails", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Add parameters
        command.Parameters.AddWithValue("@RequestId", (object?)null ?? DBNull.Value);
        command.Parameters.AddWithValue("@LoggedInEmail", (object?)filter.LoggedInEmail ?? DBNull.Value);
        command.Parameters.AddWithValue("@ViewType", (object?)filter.ViewType ?? DBNull.Value);
        command.Parameters.AddWithValue("@PendingWith", (object?)filter.PendingWith ?? DBNull.Value);
        command.Parameters.AddWithValue("@Type", (object?)filter.Type ?? DBNull.Value);
        command.Parameters.AddWithValue("@Funding", (object?)filter.Funding ?? DBNull.Value);
        command.Parameters.AddWithValue("@Level", (object?)filter.Level ?? DBNull.Value);

        // Handle requestor array
        var requestorEmails = filter.Requestor != null && filter.Requestor.Any()
            ? string.Join(",", filter.Requestor)
            : null;
        command.Parameters.AddWithValue("@RequestorEmails", (object?)requestorEmails ?? DBNull.Value);

        command.Parameters.AddWithValue("@IsPending", (object?)filter.IsPending ?? DBNull.Value);

        // Handle timeline
        if (filter.Timeline != null)
        {
            command.Parameters.AddWithValue("@TimelineRangeType", (object?)filter.Timeline.RangeType ?? DBNull.Value);
            command.Parameters.AddWithValue("@TimelineValue", (object?)filter.Timeline.Value ?? DBNull.Value);
            command.Parameters.AddWithValue("@TimelineStartDate", (object?)filter.Timeline.StartDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@TimelineEndDate", (object?)filter.Timeline.EndDate ?? DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("@TimelineRangeType", DBNull.Value);
            command.Parameters.AddWithValue("@TimelineValue", DBNull.Value);
            command.Parameters.AddWithValue("@TimelineStartDate", DBNull.Value);
            command.Parameters.AddWithValue("@TimelineEndDate", DBNull.Value);
        }

        command.Parameters.AddWithValue("@IncludeDetails", 0); // List view
        command.Parameters.AddWithValue("@PageNumber", filter.PageNumber);
        command.Parameters.AddWithValue("@PageSize", filter.PageSize);

        using var reader = await command.ExecuteReaderAsync();

        // First result set: Request list
        while (await reader.ReadAsync())
        {
            response.Data.Add(new RequestListItemResponse
            {
                RequestId = reader["RequestId"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString(),
                Requestor = reader["Requestor"]?.ToString(),
                PendingWith = reader["PendingWith"]?.ToString(),
                JobTitle = reader["JobTitle"]?.ToString(),
                Funding = reader["Funding"]?.ToString(),
                Type = reader["Type"]?.ToString(),
                Level = reader["Level"]?.ToString(),
                SubmittedOn = reader["SubmittedOn"] as DateTime?,
                CompletedOn = reader["CompletedOn"] as DateTime?,
                UniqueID = reader["UniqueID"]?.ToString()
            });
        }

        // Second result set: Pagination metadata
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            response.Pagination = new PaginationMetadata
            {
                TotalRecords = Convert.ToInt32(reader["TotalRecords"]),
                CurrentPage = Convert.ToInt32(reader["CurrentPage"]),
                PageSize = Convert.ToInt32(reader["PageSize"]),
                TotalPages = Convert.ToInt32(reader["TotalPages"]),
                HasPreviousPage = Convert.ToInt32(reader["HasPreviousPage"]) == 1,
                HasNextPage = Convert.ToInt32(reader["HasNextPage"]) == 1
            };
        }

        return response;
    }

    public async Task<RequestDetailResponse?> GetRequestDetailAsync(string requestId, string? requestType)
    {
        if (!Guid.TryParse(requestId, out var requestGuid))
        {
            return null;
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_GetRequestDetails", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Add parameters for detail view
        command.Parameters.AddWithValue("@RequestId", requestGuid);
        command.Parameters.AddWithValue("@LoggedInEmail", DBNull.Value);
        command.Parameters.AddWithValue("@ViewType", DBNull.Value);
        command.Parameters.AddWithValue("@PendingWith", DBNull.Value);
        command.Parameters.AddWithValue("@Type", (object?)requestType ?? DBNull.Value);
        command.Parameters.AddWithValue("@Funding", DBNull.Value);
        command.Parameters.AddWithValue("@Level", DBNull.Value);
        command.Parameters.AddWithValue("@RequestorEmails", DBNull.Value);
        command.Parameters.AddWithValue("@IsPending", DBNull.Value);
        command.Parameters.AddWithValue("@TimelineRangeType", DBNull.Value);
        command.Parameters.AddWithValue("@TimelineValue", DBNull.Value);
        command.Parameters.AddWithValue("@TimelineStartDate", DBNull.Value);
        command.Parameters.AddWithValue("@TimelineEndDate", DBNull.Value);
        command.Parameters.AddWithValue("@IncludeDetails", 1); // Detail view
        command.Parameters.AddWithValue("@PageNumber", 1); // Not used for detail
        command.Parameters.AddWithValue("@PageSize", 10); // Not used for detail

        using var reader = await command.ExecuteReaderAsync();

        RequestDetailResponse? response = null;

        // First result set: Request details
        if (await reader.ReadAsync())
        {
            response = new RequestDetailResponse
            {
                RequestType = reader["RequestType"]?.ToString(),
                ProjectName = reader["ProjectName"]?.ToString(),
                Rationale = reader["Rationale"]?.ToString(),
                City = reader["City"]?.ToString(),
                AI_or_DataRole = !reader.IsDBNull(reader.GetOrdinal("AI_or_DataRole")) && reader.GetBoolean(reader.GetOrdinal("AI_or_DataRole")),
                CompletedOn = reader["CompletedOn"] as DateTime?,
                CWAnnualRate = reader["CWAnnualRate"] as decimal?,
                CWHourlyRate = reader["CWHourlyRate"] as decimal?,
                HasNoIntraLevelReporting = reader["HasNoIntraLevelReporting"] as bool?,
                MeetsSpanofControlRequirements = reader["MeetsSpanOfControlRequirements"] as bool?,
                IsApplicationEngineeringInvestment = reader["IsApplicationEngineeringInvestment"] as bool?,
                IsAiGovernanceInvestment = reader["IsAiGovernanceInvestment"] as bool?,
                IsCwConversionFuture = reader["IsCwConversionFuture"] as bool?,
                CurrentStatus = reader["CurrentStatus"]?.ToString()
            };

            // Map LabelValuePair fields
            if (!reader.IsDBNull(reader.GetOrdinal("JobProfileLabel")))
            {
                response.JobProfile = new LabelValuePair
                {
                    Label = reader["JobProfileLabel"]?.ToString(),
                    Value = reader["JobProfileValue"]?.ToString()
                };
            }

            if (!reader.IsDBNull(reader.GetOrdinal("FundingLabel")))
            {
                response.Funding = new LabelValuePair
                {
                    Label = reader["FundingLabel"]?.ToString(),
                    Value = reader["FundingValue"]?.ToString()
                };
            }

            if (!reader.IsDBNull(reader.GetOrdinal("JobLevelLabel")))
            {
                response.JobLevel = new LabelValuePair
                {
                    Label = reader["JobLevelLabel"]?.ToString(),
                    Value = reader["JobLevelValue"]?.ToString()
                };
            }

            if (!reader.IsDBNull(reader.GetOrdinal("CountryLabel")))
            {
                response.Country = new LabelValuePair
                {
                    Label = reader["CountryLabel"]?.ToString(),
                    Value = reader["CountryValue"]?.ToString()
                };
            }
        }

        if (response == null)
        {
            return null;
        }

        // Second result set: Approvers
        if (await reader.NextResultAsync())
        {
            response.Approvers = new List<ApproverDetail>();
            while (await reader.ReadAsync())
            {
                response.Approvers.Add(new ApproverDetail
                {
                    UniqueID = reader["UniqueID"]?.ToString(),
                    RequestToName = reader["RequestToName"]?.ToString(),
                    RequestToEmail = reader["RequestToEmail"]?.ToString(),
                    RequestToRole = reader["RequestToRole"]?.ToString(),
                    SeqOrder = reader["SeqOrder"]?.ToString()
                });
            }
        }

        // Third result set: Approver chain activity
        if (await reader.NextResultAsync())
        {
            response.ApproverChainActivity = new List<ApproverChainActivity>();
            while (await reader.ReadAsync())
            {
                response.ApproverChainActivity.Add(new ApproverChainActivity
                {
                    ApproverName = reader["ApproverName"]?.ToString(),
                    ApproverStatus = reader["ApproverStatus"]?.ToString(),
                    ApprovedBy = reader["ApprovedBy"]?.ToString(),
                    ApprovedOn = reader["ApprovedOn"] as DateTime?,
                    RejectedBy = reader["RejectedBy"]?.ToString(),
                    RejectedOn = reader["RejectedOn"] as DateTime?,
                    CancelledOn = reader["CancelledOn"] as DateTime?,
                    CancelledBy = reader["CancelledBy"]?.ToString(),
                    ApproverRole = reader["ApproverRole"]?.ToString(),
                    SeqOrder = reader["SeqOrder"]?.ToString(),
                    RequestedOn = reader["RequestedOn"] as DateTime?,
                    IsBehalfof = reader["IsApprover"] as bool? ?? false,
                    ReturnedBy = reader["ReturnedBy"]?.ToString(),
                    ReturnedOn = reader["ReturnedOn"] as DateTime?
                });
            }
        }

        // Fourth result set: Comments
        if (await reader.NextResultAsync())
        {
            response.CommentsList = new List<CommentDetail>();
            while (await reader.ReadAsync())
            {
                response.CommentsList.Add(new CommentDetail
                {
                    ApproverName = reader["ApproverName"]?.ToString(),
                    CommentedOn = reader["CommentedOn"] as DateTime?,
                    ApproverStatus = reader["ApproverStatus"]?.ToString(),
                    Comments = reader["Comments"]?.ToString()
                });
            }
        }

        return response;
    }
}
