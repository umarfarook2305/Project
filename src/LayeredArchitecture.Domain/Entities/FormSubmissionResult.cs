namespace HART.Domain.Entities;

public class FormSubmissionResult
{
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FailedStep { get; set; }
}
