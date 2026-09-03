using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Domain.Entities;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Application.Departments.CreateDepartment;

public sealed class CreateDepartmentHandler
{
    private readonly IDepartmentRepository _departmentRepository;

    public CreateDepartmentHandler(IDepartmentRepository departmentRepository)
    {
        ArgumentNullException.ThrowIfNull(departmentRepository);

        _departmentRepository = departmentRepository;
    }

    public async Task<DepartmentDto> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var department = new Department(
            new DepartmentName(command.Name),
            command.Description);

        await _departmentRepository.AddAsync(department, cancellationToken);

        return new DepartmentDto(
            department.Id,
            department.Name.Value,
            department.Description);
    }
}
