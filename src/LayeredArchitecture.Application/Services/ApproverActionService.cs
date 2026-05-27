using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Approver Action operations
/// </summary>
public class ApproverActionService : IApproverActionService
{
    private readonly IApproverActionRepository _repository;
    private static readonly List<string> ValidActions = new() { "Approved", "Rejected", "Returned to Requestor" };

    public ApproverActionService(IApproverActionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApproverActionResponse> ProcessApproverActionAsync(ApproverActionRequest request)
    {
        // Validate request
        ValidateRequest(request);

        return await _repository.ProcessApproverActionAsync(request);
    }

    private static void ValidateRequest(ApproverActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RequestID))
        {
            throw new ArgumentException("RequestID is required");
        }

        if (!Guid.TryParse(request.RequestID, out _))
        {
            throw new ArgumentException("RequestID must be a valid GUID");
        }

        if (string.IsNullOrWhiteSpace(request.RequestorEmail))
        {
            throw new ArgumentException("RequestorEmail is required");
        }

        if (string.IsNullOrWhiteSpace(request.ApproverName))
        {
            throw new ArgumentException("ApproverName is required");
        }

        if (string.IsNullOrWhiteSpace(request.ApproverEmail))
        {
            throw new ArgumentException("ApproverEmail is required");
        }

        if (string.IsNullOrWhiteSpace(request.ApproverAction))
        {
            throw new ArgumentException("ApproverAction is required");
        }

        if (!ValidActions.Contains(request.ApproverAction))
        {
            throw new ArgumentException($"ApproverAction must be one of: {string.Join(", ", ValidActions)}");
        }
    }
}
