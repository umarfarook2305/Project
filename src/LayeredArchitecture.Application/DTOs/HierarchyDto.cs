namespace HART.Application.DTOs;

/// <summary>
/// Request model for getting employee hierarchy
/// </summary>
public class HierarchyRequest
{
    /// <summary>
    /// Email address of the employee to get hierarchy for
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Response model for employee hierarchy
/// </summary>
public class HierarchyResponse
{
    /// <summary>
    /// Status of the request
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Number of hierarchy levels returned
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// List of managers in hierarchical order
    /// </summary>
    public List<HierarchyLevel> Data { get; set; } = new();
}

/// <summary>
/// Individual level in the hierarchy
/// </summary>
public class HierarchyLevel
{
    /// <summary>
    /// Level number (1 = direct manager, 2 = manager's manager, etc.)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Manager's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Manager's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Manager's job title
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
