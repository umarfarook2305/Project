namespace HART.Application.DTOs;

/// <summary>
/// Response for Position ID endpoint
/// </summary>
public class PositionResponse
{
    public string Status { get; set; } = "success";
    public int Count { get; set; }
    public List<PositionData> Data { get; set; } = new();
}

/// <summary>
/// Position data item
/// </summary>
public class PositionData
{
    public string? EmployeeName { get; set; }
    public string? EmployeeGuid { get; set; }
    public string? PositionId { get; set; }
    public string? PositionIdGuid { get; set; }
    public string? PositionStatus { get; set; }
    public string? JobRequisitionId { get; set; }
    public string? JobRequisitionGuid { get; set; }
    public string? JobProfileName { get; set; }
}
