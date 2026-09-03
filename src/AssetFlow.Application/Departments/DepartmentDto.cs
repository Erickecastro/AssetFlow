namespace AssetFlow.Application.Departments;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string? Description);
