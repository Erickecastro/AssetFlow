namespace AssetFlow.Application.Departments.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string? Description = null);
