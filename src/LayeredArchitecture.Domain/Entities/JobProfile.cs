namespace HART.Domain.Entities;

public class JobProfile
{
    public string ProfileId { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string? FamilyId { get; set; }
    public string? FamilyName { get; set; }
    public string? JobLevelId { get; set; }
    public string? JobLevelName { get; set; }
}
