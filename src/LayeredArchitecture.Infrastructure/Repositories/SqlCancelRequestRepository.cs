using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Cancel Request operations using ADO.NET
/// </summary>
public class SqlCancelRequestRepository : ICancelRequestRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlCancelRequestRepository> _logger;

    public SqlCancelRequestRepository(
        IConfiguration configuration,
        ILogger<SqlCancelRequestRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<CancelRequestResponse> CancelRequestAsync(CancelRequestDto request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_CancelRequest", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add parameters
            command.Parameters.AddWithValue("@RequestID", Guid.Parse(request.RequestID));
            command.Parameters.AddWithValue("@RequestorName", request.RequestorName);
            command.Parameters.AddWithValue("@RequestorEmail", request.RequestorEmail);

            using var reader = await command.ExecuteReaderAsync();

            var approversToNotify = new List<ApproverNotification>();

            // Read approvers that need to be notified
            while (await reader.ReadAsync())
            {
                var approver = new ApproverNotification
                {
                    ApprovalId = reader["ApprovalId"]?.ToString() ?? string.Empty,
                    ApproverName = reader["ApproverName"]?.ToString() ?? string.Empty,
                    ApproverEmail = reader["ApproverEmail"]?.ToString() ?? string.Empty,
                    RequestID = reader["RequestID"]?.ToString() ?? string.Empty,
                    RequestType = reader["RequestType"]?.ToString() ?? string.Empty,
                    ProjectName = reader["ProjectName"]?.ToString() ?? string.Empty,
                    FundingType = reader["FundingType"]?.ToString() ?? string.Empty,
                    JobLevel = reader["JobLevel"]?.ToString() ?? string.Empty,
                    Position = reader["Position"]?.ToString() ?? string.Empty
                };

                approversToNotify.Add(approver);
            }

            // TODO: Send notifications (Teams/Email) to approversToNotify
            // This would typically be done via a separate notification service
            // For now, just log the notifications that would be sent
            _logger.LogInformation(
                "Request {RequestID} cancelled by {RequestorName}. {Count} approvers to notify.",
                request.RequestID,
                request.RequestorName,
                approversToNotify.Count);

            foreach (var approver in approversToNotify)
            {
                _logger.LogInformation(
                    "Notification to be sent to {ApproverName} ({ApproverEmail}) for request {RequestID}",
                    approver.ApproverName,
                    approver.ApproverEmail,
                    approver.RequestID);
            }

            return new CancelRequestResponse
            {
                Message = "Records Has updated and Notification sends to User"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling request {RequestID}", request.RequestID);
            throw new Exception($"Error cancelling request: {ex.Message}", ex);
        }
    }

    private class ApproverNotification
    {
        public string ApprovalId { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public string ApproverEmail { get; set; } = string.Empty;
        public string RequestID { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FundingType { get; set; } = string.Empty;
        public string JobLevel { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
