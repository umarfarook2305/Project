using HART.Application.DTOs;
using HART.Domain.Entities;
using HART.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

namespace HART.Infrastructure.Repositories;

public class SqlFormSubmissionRepository : IFormSubmissionRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlFormSubmissionRepository>? _logger;

    public SqlFormSubmissionRepository(
        IConfiguration configuration,
        ILogger<SqlFormSubmissionRepository>? logger = null)
    {
        _connectionString = configuration.GetConnectionString("HARTDatabase")
            ?? throw new InvalidOperationException("Connection string 'HARTDatabase' not found.");
        _logger = logger;
    }

    public async Task<FormSubmissionResult> SubmitRequestAsync(dynamic requestData)
    {
        // Convert dynamic to FormSubmissionRequest
        FormSubmissionRequest request = requestData;
        var result = new FormSubmissionResult { Success = false };

        try
        {
            _logger?.LogInformation("Submitting {RequestType} request for {RequestedBy}",
                request.Request.RequestType, request.Request.RequestedByEmail);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("dbo.usp_SubmitFormRequest", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add request parameters
            AddParameter(command, "@RequestId", null);  // Always insert new (let SP generate)
            AddParameter(command, "@RequestType", request.Request.RequestType);
            AddParameter(command, "@RequestedOn", request.Request.RequestedOn);
            AddParameter(command, "@CurrentStatusId", request.Request.CurrentStatusId);
            AddParameter(command, "@PendingWithEmail", request.Request.PendingWithEmail);
            AddParameter(command, "@PendingWithName", request.Request.PendingWithName);
            AddParameter(command, "@RequestBy", request.Request.RequestedByEmail);
            AddParameter(command, "@RequestByName", request.Request.RequestedByName);

            AddParameter(command, "@PositionTypeId", request.Request.PositionTypeId);
            AddParameter(command, "@JobLevelId", request.Request.JobLevelId);
            AddParameter(command, "@FundingTypeId", request.Request.FundingTypeId);
            AddParameter(command, "@VacancyTypeId", request.Request.StatusOfPositionId);
            AddParameter(command, "@RoleTypeId", request.Request.RoleTypeId);
            AddParameter(command, "@SWPRoleId", request.Request.SwpRoleId);
            AddParameter(command, "@JobFamilyId", request.Request.JobFamilyId);
            AddParameter(command, "@JobProfileId", request.Request.JobProfileId);
            AddParameter(command, "@CountryId", request.Request.CountryId);
            AddParameter(command, "@City", request.Request.CityId);
            AddParameter(command, "@PositionId", request.Request.PositionId);

            AddParameter(command, "@LineOfBusinessId", request.Request.LineOfBusinessId);
            AddParameter(command, "@ProjectThemeId", request.Request.ProjectThemeId);
            AddParameter(command, "@ProjectName", request.Request.ProjectName);
            AddParameter(command, "@Rationale", request.Request.Rationale);

            AddParameter(command, "@ReplacementEmployeeId", request.Request.ReplacementEmployeeEmailId);
            AddParameter(command, "@PromotedEmployeeId", request.Request.PromotedEmployeeEmailId);
            AddParameter(command, "@CWEmployeeEmailId", request.Request.CWEmployeeEmailId);

            AddParameter(command, "@CWAnnualRate", request.Request.CWAnnualRate);
            AddParameter(command, "@CWHourlyRate", request.Request.CWHourlyRate);

            AddParameter(command, "@IsAIDataRole", request.Request.IsAiDataRole);
            AddParameter(command, "@IsAIGovernanceInvestment", request.Request.IsAiGovernanceInvestment);
            AddParameter(command, "@IsApplicationEngineeringInvest", request.Request.IsApplicationEngineeringInvestment);
            AddParameter(command, "@IsCWConversionFuture", request.Request.IsCwConversionFuture);
            AddParameter(command, "@IsNewPosition", request.Request.IsNewPosition);
            AddParameter(command, "@HasNoIntraLevelReporting", request.Request.HasNoIntraLevelReporting);
            AddParameter(command, "@MeetsSpanOfControlRequirements", request.Request.MeetsSpanOfControlRequirements);

            // Serialize approvals to JSON
            var approvalsJson = JsonSerializer.Serialize(request.Approvals, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            AddParameter(command, "@ApprovalsJSON", approvalsJson);

            // Output parameters
            var outRequestIdParam = command.Parameters.Add("@OutRequestId", SqlDbType.UniqueIdentifier);
            outRequestIdParam.Direction = ParameterDirection.Output;

            var outRequestCodeParam = command.Parameters.Add("@OutRequestCode", SqlDbType.NVarChar, 50);
            outRequestCodeParam.Direction = ParameterDirection.Output;

            // Execute stored procedure
            using var reader = await command.ExecuteReaderAsync();

            // Read result set
            if (await reader.ReadAsync())
            {
                var status = reader["Status"]?.ToString();

                if (status == "success")
                {
                    result.Success = true;
                    result.RequestId = reader["RequestId"]?.ToString() ?? string.Empty;

                    _logger?.LogInformation("Successfully submitted request. RequestId: {RequestId}, RequestCode: {RequestCode}",
                        result.RequestId, reader["RequestCode"]?.ToString());
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = reader["Error"]?.ToString() ?? "Unknown error";
                    result.FailedStep = reader["FailedStep"]?.ToString() ?? "Unknown";

                    _logger?.LogError("Request submission failed: {Error}", result.ErrorMessage);
                }
            }

            return result;
        }
        catch (SqlException sqlEx)
        {
            _logger?.LogError(sqlEx, "SQL error submitting request");
            result.ErrorMessage = $"Database error: {sqlEx.Message}";
            result.FailedStep = "Database Operation";
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error submitting request");
            result.ErrorMessage = ex.Message;
            result.FailedStep = "Request Submission";
            return result;
        }
    }

    private void AddParameter(SqlCommand command, string parameterName, object? value)
    {
        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            command.Parameters.AddWithValue(parameterName, DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue(parameterName, value);
        }
    }

    public async Task<FormSubmissionResult> UpdateRequestAsync(string requestId, dynamic requestData)
    {
        // Convert dynamic to FormSubmissionRequest
        FormSubmissionRequest request = requestData;
        var result = new FormSubmissionResult { Success = false };

        try
        {
            _logger?.LogInformation("Updating {RequestType} request {RequestId} for {RequestedBy}",
                request.Request.RequestType, requestId, request.Request.RequestedByEmail);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // First, check if request exists
            using (var checkCommand = new SqlCommand("SELECT COUNT(1) FROM [Request] WHERE RequestId = @RequestId", connection))
            {
                checkCommand.Parameters.AddWithValue("@RequestId", Guid.Parse(requestId));
                var exists = (int)await checkCommand.ExecuteScalarAsync();

                if (exists == 0)
                {
                    result.ErrorMessage = $"Request with ID '{requestId}' not found";
                    result.FailedStep = "Validation";
                    _logger?.LogWarning("Request not found: {RequestId}", requestId);
                    return result;
                }
            }

            using var command = new SqlCommand("dbo.usp_SubmitFormRequest", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Add request parameters (with RequestId for update)
            AddParameter(command, "@RequestId", Guid.Parse(requestId));  // Provide RequestId for update
            AddParameter(command, "@RequestType", request.Request.RequestType);
            AddParameter(command, "@RequestedOn", request.Request.RequestedOn);
            AddParameter(command, "@CurrentStatusId", request.Request.CurrentStatusId);
            AddParameter(command, "@PendingWithEmail", request.Request.PendingWithEmail);
            AddParameter(command, "@PendingWithName", request.Request.PendingWithName);
            AddParameter(command, "@RequestBy", request.Request.RequestedByEmail);
            AddParameter(command, "@RequestByName", request.Request.RequestedByName);

            AddParameter(command, "@PositionTypeId", request.Request.PositionTypeId);
            AddParameter(command, "@JobLevelId", request.Request.JobLevelId);
            AddParameter(command, "@FundingTypeId", request.Request.FundingTypeId);
            AddParameter(command, "@VacancyTypeId", request.Request.StatusOfPositionId);
            AddParameter(command, "@RoleTypeId", request.Request.RoleTypeId);
            AddParameter(command, "@SWPRoleId", request.Request.SwpRoleId);
            AddParameter(command, "@JobFamilyId", request.Request.JobFamilyId);
            AddParameter(command, "@JobProfileId", request.Request.JobProfileId);
            AddParameter(command, "@CountryId", request.Request.CountryId);
            AddParameter(command, "@City", request.Request.CityId);
            AddParameter(command, "@PositionId", request.Request.PositionId);

            AddParameter(command, "@LineOfBusinessId", request.Request.LineOfBusinessId);
            AddParameter(command, "@ProjectThemeId", request.Request.ProjectThemeId);
            AddParameter(command, "@ProjectName", request.Request.ProjectName);
            AddParameter(command, "@Rationale", request.Request.Rationale);

            AddParameter(command, "@ReplacementEmployeeId", request.Request.ReplacementEmployeeEmailId);
            AddParameter(command, "@PromotedEmployeeId", request.Request.PromotedEmployeeEmailId);
            AddParameter(command, "@CWEmployeeEmailId", request.Request.CWEmployeeEmailId);

            AddParameter(command, "@CWAnnualRate", request.Request.CWAnnualRate);
            AddParameter(command, "@CWHourlyRate", request.Request.CWHourlyRate);

            AddParameter(command, "@IsAIDataRole", request.Request.IsAiDataRole);
            AddParameter(command, "@IsAIGovernanceInvestment", request.Request.IsAiGovernanceInvestment);
            AddParameter(command, "@IsApplicationEngineeringInvest", request.Request.IsApplicationEngineeringInvestment);
            AddParameter(command, "@IsCWConversionFuture", request.Request.IsCwConversionFuture);
            AddParameter(command, "@IsNewPosition", request.Request.IsNewPosition);
            AddParameter(command, "@HasNoIntraLevelReporting", request.Request.HasNoIntraLevelReporting);
            AddParameter(command, "@MeetsSpanOfControlRequirements", request.Request.MeetsSpanOfControlRequirements);

            // Serialize approvals to JSON
            var approvalsJson = JsonSerializer.Serialize(request.Approvals, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            AddParameter(command, "@ApprovalsJSON", approvalsJson);

            // Output parameters
            var outRequestIdParam = command.Parameters.Add("@OutRequestId", SqlDbType.UniqueIdentifier);
            outRequestIdParam.Direction = ParameterDirection.Output;

            var outRequestCodeParam = command.Parameters.Add("@OutRequestCode", SqlDbType.NVarChar, 50);
            outRequestCodeParam.Direction = ParameterDirection.Output;

            // Execute stored procedure
            using var reader = await command.ExecuteReaderAsync();

            // Read result set
            if (await reader.ReadAsync())
            {
                var status = reader["Status"]?.ToString();

                if (status == "success")
                {
                    result.Success = true;
                    result.RequestId = reader["RequestId"]?.ToString() ?? string.Empty;

                    _logger?.LogInformation("Successfully updated request. RequestId: {RequestId}, RequestCode: {RequestCode}",
                        result.RequestId, reader["RequestCode"]?.ToString());
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = reader["Error"]?.ToString() ?? "Unknown error";
                    result.FailedStep = reader["FailedStep"]?.ToString() ?? "Unknown";

                    _logger?.LogError("Request update failed: {Error}", result.ErrorMessage);
                }
            }

            return result;
        }
        catch (SqlException sqlEx)
        {
            _logger?.LogError(sqlEx, "SQL error updating request");
            result.ErrorMessage = $"Database error: {sqlEx.Message}";
            result.FailedStep = "Database Operation";
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating request");
            result.ErrorMessage = ex.Message;
            result.FailedStep = "Request Update";
            return result;
        }
    }
}
