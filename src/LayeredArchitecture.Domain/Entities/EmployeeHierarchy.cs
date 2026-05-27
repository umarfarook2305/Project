namespace HART.Domain.Entities;

public class EmployeeHierarchy
{
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
