using HART.Application.DTOs;
using HART.Application.Interfaces;
using HART.Domain.Interfaces;

namespace HART.Application.Services;

public class FormSubmissionService : IFormSubmissionService
{
    private readonly IFormSubmissionRepository _repository;

    public FormSubmissionService(IFormSubmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormSubmissionResponse> SubmitRequestAsync(FormSubmissionRequest request)
    {
        // Validate request
        ValidateRequest(request);

        // Submit to repository (pass as dynamic to avoid layer dependency)
        var result = await _repository.SubmitRequestAsync((dynamic)request);

        // Map to response
        return new FormSubmissionResponse
        {
            Status = result.Success ? "success" : "error",
            RequestId = result.RequestId,
            Error = result.ErrorMessage ?? string.Empty,
            FailedStep = result.FailedStep ?? string.Empty
        };
    }

    public async Task<FormSubmissionResponse> UpdateRequestAsync(string requestId, FormSubmissionRequest request)
    {
        // Validate requestId
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("RequestId is required", nameof(requestId));

        if (!Guid.TryParse(requestId, out _))
            throw new ArgumentException("RequestId must be a valid GUID", nameof(requestId));

        // Validate request
        ValidateRequest(request);

        // Update in repository (pass requestId and request)
        var result = await _repository.UpdateRequestAsync(requestId, (dynamic)request);

        // Map to response
        return new FormSubmissionResponse
        {
            Status = result.Success ? "success" : "error",
            RequestId = result.RequestId,
            Error = result.ErrorMessage ?? string.Empty,
            FailedStep = result.FailedStep ?? string.Empty
        };
    }

    private void ValidateRequest(FormSubmissionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");

        if (request.Request == null)
            throw new ArgumentException("Request details are required", nameof(request));

        var req = request.Request;

        // Validate common required fields
        if (string.IsNullOrWhiteSpace(req.RequestType))
            throw new ArgumentException("RequestType is required (FTE or CW)");

        if (req.RequestType != "FTE" && req.RequestType != "CW")
            throw new ArgumentException("RequestType must be either 'FTE' or 'CW'");

        if (string.IsNullOrWhiteSpace(req.FundingTypeId))
            throw new ArgumentException("FundingTypeId is required");

        if (string.IsNullOrWhiteSpace(req.CountryId))
            throw new ArgumentException("CountryId is required");

        if (string.IsNullOrWhiteSpace(req.CityId))
            throw new ArgumentException("CityId is required");

        if (string.IsNullOrWhiteSpace(req.Rationale))
            throw new ArgumentException("Rationale is required");

        if (string.IsNullOrWhiteSpace(req.CurrentStatusId))
            throw new ArgumentException("CurrentStatusId is required");

        if (string.IsNullOrWhiteSpace(req.RequestedByEmail))
            throw new ArgumentException("RequestedByEmail is required");

        if (string.IsNullOrWhiteSpace(req.RequestedByName))
            throw new ArgumentException("RequestedByName is required");

        if (string.IsNullOrWhiteSpace(req.PendingWithEmail))
            throw new ArgumentException("PendingWithEmail is required");

        if (string.IsNullOrWhiteSpace(req.PendingWithName))
            throw new ArgumentException("PendingWithName is required");

        // FTE-specific validation
        if (req.RequestType == "FTE")
        {
            if (string.IsNullOrWhiteSpace(req.JobLevelId))
                throw new ArgumentException("JobLevelId is required for FTE requests");

            if (string.IsNullOrWhiteSpace(req.PositionTypeId))
                throw new ArgumentException("PositionTypeId is required for FTE requests");

            if (string.IsNullOrWhiteSpace(req.StatusOfPositionId))
                throw new ArgumentException("StatusOfPositionId is required for FTE requests");

            if (string.IsNullOrWhiteSpace(req.RoleTypeId))
                throw new ArgumentException("RoleTypeId is required for FTE requests");

            if (!req.IsNewPosition.HasValue)
                throw new ArgumentException("IsNewPosition is required for FTE requests");

            // CW Conversion requires hourly/annual rates
            // Check if StatusOfPosition is "CW Conversion" (you'll need the actual ID or name)
            // For now, validate that if rates are provided, both should be provided
            if (req.CWHourlyRate.HasValue || req.CWAnnualRate.HasValue)
            {
                if (!req.CWHourlyRate.HasValue || !req.CWAnnualRate.HasValue)
                    throw new ArgumentException("Both CWHourlyRate and CWAnnualRate are required for CW conversion");
            }
        }

        // CW-specific validation
        if (req.RequestType == "CW")
        {
            if (string.IsNullOrWhiteSpace(req.SwpRoleId))
                throw new ArgumentException("SwpRoleId is required for CW requests");

            if (string.IsNullOrWhiteSpace(req.LineOfBusinessId))
                throw new ArgumentException("LineOfBusinessId is required for CW requests");

            if (string.IsNullOrWhiteSpace(req.ProjectThemeId))
                throw new ArgumentException("ProjectThemeId is required for CW requests");

            if (!req.IsApplicationEngineeringInvestment.HasValue)
                throw new ArgumentException("IsApplicationEngineeringInvestment is required for CW requests");

            if (!req.IsAiGovernanceInvestment.HasValue)
                throw new ArgumentException("IsAiGovernanceInvestment is required for CW requests");

            if (!req.IsCwConversionFuture.HasValue)
                throw new ArgumentException("IsCwConversionFuture is required for CW requests");
        }

        // Validate approvals
        if (request.Approvals == null || request.Approvals.Count == 0)
            throw new ArgumentException("At least one approval is required");

        foreach (var approval in request.Approvals)
        {
            if (string.IsNullOrWhiteSpace(approval.PositionStatusId))
                throw new ArgumentException("PositionStatusId is required for all approvals");

            if (string.IsNullOrWhiteSpace(approval.RequestedTo))
                throw new ArgumentException("RequestedTo is required for all approvals");

            if (string.IsNullOrWhiteSpace(approval.RequestedToEmail))
                throw new ArgumentException("RequestedToEmail is required for all approvals");

            if (string.IsNullOrWhiteSpace(approval.RequestedToRole))
                throw new ArgumentException("RequestedToRole is required for all approvals");

            if (approval.SequenceOrder <= 0)
                throw new ArgumentException("SequenceOrder must be greater than 0");
        }
    }
}
