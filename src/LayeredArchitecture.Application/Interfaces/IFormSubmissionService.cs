using HART.Application.DTOs;

namespace HART.Application.Interfaces;

public interface IFormSubmissionService
{
    /// <summary>
    /// Submit FTE or CW request with validation and approval workflow
    /// </summary>
    /// <param name="request">Form submission request</param>
    /// <returns>Response with status, request ID, or error details</returns>
    Task<FormSubmissionResponse> SubmitRequestAsync(FormSubmissionRequest request);

    /// <summary>
    /// Update existing FTE or CW request with validation and approval workflow
    /// </summary>
    /// <param name="requestId">The ID of the request to update</param>
    /// <param name="request">Form submission request with updated data</param>
    /// <returns>Response with status, request ID, or error details</returns>
    Task<FormSubmissionResponse> UpdateRequestAsync(string requestId, FormSubmissionRequest request);
}
