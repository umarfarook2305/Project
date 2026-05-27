using HART.Application.DTOs;
using HART.Application.Interfaces;

namespace HART.Application.Services;

/// <summary>
/// Service for Position operations
/// </summary>
public class PositionService : IPositionService
{
    private readonly IPositionRepository _repository;

    public PositionService(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PositionResponse> GetPositionsAsync()
    {
        return await _repository.GetPositionsAsync();
    }
}
