using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

public class InsertDelegateService : IInsertDelegateService
{
    private readonly IInsertDelegateRepository _repository;

    public InsertDelegateService(IInsertDelegateRepository repository)
    {
        _repository = repository;
    }

    public async Task<InsertDelegateResponse> InsertDelegateAsync(InsertDelegateRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.DelegateUserEmail))
            throw new ArgumentException("DelegateUserEmail is required");

        if (string.IsNullOrWhiteSpace(request.BehalfUserEmail))
            throw new ArgumentException("BehalfUserEmail is required");

        if (string.IsNullOrWhiteSpace(request.DelegateFrom))
            throw new ArgumentException("DelegateFrom is required");

        if (string.IsNullOrWhiteSpace(request.DelegateTo))
            throw new ArgumentException("DelegateTo is required");

        // Validate date formats
        if (!DateTime.TryParse(request.DelegateFrom, out var fromDate))
            throw new ArgumentException("DelegateFrom must be a valid date");

        if (!DateTime.TryParse(request.DelegateTo, out var toDate))
            throw new ArgumentException("DelegateTo must be a valid date");

        // Validate date range
        if (toDate < fromDate)
            throw new ArgumentException("DelegateTo must be greater than or equal to DelegateFrom");

        return await _repository.InsertDelegateAsync(request);
    }
}
