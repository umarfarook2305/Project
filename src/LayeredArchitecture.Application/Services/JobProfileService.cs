using HART.Application.DTOs;
using HART.Application.Interfaces;
using HART.Domain.Interfaces;

namespace HART.Application.Services;

public class JobProfileService : IJobProfileService
{
    private readonly IJobProfileRepository _repository;

    public JobProfileService(IJobProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<JobProfileResponse>> GetJobProfilesAsync(string? jobLevel = null)
    {
        var profiles = await _repository.GetJobProfilesAsync(jobLevel);

        var response = profiles
            .Where(p => !string.IsNullOrWhiteSpace(p.ProfileName))
            .Select(p => new JobProfileResponse
            {
                Profile = p.ProfileName,
                Family = p.FamilyName,
                ProfileID = p.ProfileId,
                FamilyID = p.FamilyId
            })
            .ToList();

        return response;
    }
}
