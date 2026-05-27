namespace HART.Application.DTOs;

/// <summary>
/// Request to cancel a position request
/// </summary>
public class CancelRequestDto
{
    public string RequestID { get; set; } = string.Empty;
    public string RequestorName { get; set; } = string.Empty;
    public string RequestorEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response for cancel request operation
/// </summary>
public class CancelRequestResponse
{
    public string Message { get; set; } = string.Empty;
}
