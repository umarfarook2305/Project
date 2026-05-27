using HART.Domain.Entities;

namespace HART.Domain.Interfaces;

public interface IFormSubmissionRepository
{
    /// <summary>
    /// Submit FTE or CW request with approvals
    /// </summary>
    /// <param name="requestData">Form submission request containing request details and approvals (dynamic object)</param>
    /// <returns>Result with request ID or error information</returns>
    Task<FormSubmissionResult> SubmitRequestAsync(dynamic requestData);

    /// <summary>
    /// Update existing FTE or CW request with approvals
    /// </summary>
    /// <param name="requestId">The ID of the request to update</param>
    /// <param name="requestData">Form submission request containing updated request details and approvals (dynamic object)</param>
    /// <returns>Result with request ID or error information</returns>
    Task<FormSubmissionResult> UpdateRequestAsync(string requestId, dynamic requestData);
}
