using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;

namespace HART.Infrastructure.Repositories;

public class SqlResubmitRequestRepository : IResubmitRequestRepository
{
    private readonly string _connectionString;

    public SqlResubmitRequestRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<ResubmitResponse> ResubmitRequestAsync(ResubmitRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_ResubmitRequest", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };

            // Add request parameters (all nullable except RequestId)
            command.Parameters.AddWithValue("@RequestId", Guid.Parse(request.Request.RequestId));
            AddNullableGuidParameter(command, "@JobProfileId", request.Request.JobProfileId);
            AddNullableGuidParameter(command, "@LineOfBusinessId", request.Request.LineOfBusinessId);
            AddNullableParameter(command, "@ProjectName", request.Request.ProjectName);
            AddNullableGuidParameter(command, "@ProjectThemeId", request.Request.ProjectThemeId);
            AddNullableGuidParameter(command, "@FundingTypeId", request.Request.FundingTypeId);
            AddNullableGuidParameter(command, "@SWPRoleId", request.Request.SwpRoleId);
            AddNullableGuidParameter(command, "@JobFamilyId", request.Request.JobFamilyId);
            AddNullableGuidParameter(command, "@CountryId", request.Request.CountryId);
            AddNullableParameter(command, "@City", request.Request.CityId);
            AddNullableParameter(command, "@Rationale", request.Request.Rationale);
            AddNullableGuidParameter(command, "@CurrentStatusId", request.Request.CurrentStatusId);
            AddNullableParameter(command, "@RequestType", request.Request.RequestType);
            AddNullableBitParameter(command, "@IsAIDataRole", request.Request.IsAiDataRole);
            AddNullableBitParameter(command, "@IsApplicationEngineeringInvest", request.Request.IsApplicationEngineeringInvestment);
            AddNullableBitParameter(command, "@IsAIGovernanceInvestment", request.Request.IsAiGovernanceInvestment);
            AddNullableBitParameter(command, "@IsCWConversionFuture", request.Request.IsCwConversionFuture);
            AddNullableDateTimeParameter(command, "@RequestedOn", request.Request.RequestedOn);
            AddNullableParameter(command, "@PendingWithName", request.Request.PendingWithName);
            AddNullableParameter(command, "@PendingWithEmail", request.Request.PendingWithEmail);
            AddNullableParameter(command, "@RequestedByEmail", request.Request.RequestedByEmail);
            AddNullableParameter(command, "@RequestedByName", request.Request.RequestedByName);
            AddNullableGuidParameter(command, "@JobLevelId", request.Request.JobLevelId);
            AddNullableGuidParameter(command, "@PositionTypeId", request.Request.PositionTypeId);
            AddNullableGuidParameter(command, "@PositionId", request.Request.PositionId);
            AddNullableGuidParameter(command, "@RoleTypeId", request.Request.RoleTypeId);
            AddNullableGuidParameter(command, "@VacancyTypeId", request.Request.VacancyTypeId);
            AddNullableGuidParameter(command, "@PromotedEmployeeId", request.Request.PromotedEmployeeEmailId);
            AddNullableGuidParameter(command, "@ReplacementEmployeeId", request.Request.ReplacementEmployeeEmailId);
            AddNullableGuidParameter(command, "@CWEmployeeEmailId", request.Request.CwEmployeeEmailId);
            AddNullableDecimalParameter(command, "@CWAnnualRate", request.Request.CwAnnualRate);
            AddNullableDecimalParameter(command, "@CWHourlyRate", request.Request.CwHourlyRate);
            AddNullableBitParameter(command, "@HasNoIntraLevelReporting", request.Request.HasNoIntraLevelReporting);
            AddNullableBitParameter(command, "@IsNewPosition", request.Request.IsNewPostion);
            AddNullableBitParameter(command, "@MeetsSpanOfControlRequirements", request.Request.MeetsSpanOfControlRequirements);

            // Serialize approvals to JSON
            var approvalsJson = JsonSerializer.Serialize(request.Approvals);
            command.Parameters.AddWithValue("@ApprovalsJSON", approvalsJson);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"]?.ToString() ?? "500";
                var status = reader["Status"]?.ToString() ?? "error";
                var message = reader["Message"]?.ToString() ?? "Unknown error";
                var requestId = reader.GetOrdinal("RequestId") >= 0 && !reader.IsDBNull(reader.GetOrdinal("RequestId"))
                    ? reader["RequestId"]?.ToString()
                    : null;

                return new ResubmitResponse
                {
                    StatusCode = statusCode,
                    Body = new ResubmitBody
                    {
                        Status = status,
                        Message = message,
                        RequestId = requestId
                    }
                };
            }

            return new ResubmitResponse
            {
                StatusCode = "500",
                Body = new ResubmitBody
                {
                    Status = "error",
                    Message = "Failed to resubmit request"
                }
            };
        }
        catch
        {
            return new ResubmitResponse
            {
                StatusCode = "500",
                Body = new ResubmitBody
                {
                    Status = "error",
                    Message = "Failed to resubmit request"
                }
            };
        }
    }

    private static void AddNullableParameter(SqlCommand command, string paramName, string? value)
    {
        command.Parameters.AddWithValue(paramName, string.IsNullOrWhiteSpace(value) ? DBNull.Value : value);
    }

    private static void AddNullableGuidParameter(SqlCommand command, string paramName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var guid))
            command.Parameters.AddWithValue(paramName, DBNull.Value);
        else
            command.Parameters.AddWithValue(paramName, guid);
    }

    private static void AddNullableBitParameter(SqlCommand command, string paramName, bool? value)
    {
        command.Parameters.AddWithValue(paramName, value.HasValue ? value.Value : DBNull.Value);
    }

    private static void AddNullableIntParameter(SqlCommand command, string paramName, int? value)
    {
        command.Parameters.AddWithValue(paramName, value.HasValue ? value.Value : DBNull.Value);
    }

    private static void AddNullableDecimalParameter(SqlCommand command, string paramName, decimal? value)
    {
        command.Parameters.AddWithValue(paramName, value.HasValue ? value.Value : DBNull.Value);
    }

    private static void AddNullableDateTimeParameter(SqlCommand command, string paramName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value, out var dateTime))
            command.Parameters.AddWithValue(paramName, DBNull.Value);
        else
            command.Parameters.AddWithValue(paramName, dateTime);
    }
}
