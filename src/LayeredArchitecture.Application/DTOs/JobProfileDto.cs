namespace HART.Application.DTOs;

/// <summary>
/// Request model for getting job profiles
/// </summary>
public class JobProfileRequest
{
    /// <summary>
    /// Job Level filter (optional - if empty, returns all profiles)
    /// Can be JobLevel name, ID, or code
    /// </summary>
    public string? JobLevel { get; set; }
}

/// <summary>
/// Response model for job profile data
/// </summary>
public class JobProfileResponse
{
    public string? Profile { get; set; }
    public string? Family { get; set; }
    public string? ProfileID { get; set; }
    public string? FamilyID { get; set; }
}
