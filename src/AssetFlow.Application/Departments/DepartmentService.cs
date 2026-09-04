using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Application.Common;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Application.Departments;

public sealed class DepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DepartmentDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await _repository.ListAsync(cancellationToken);

        return departments
            .Select(department => new DepartmentDto(
                department.Id,
                department.Name.Value,
                department.Description))
            .ToArray();
    }

    public async Task<DepartmentDto> UpdateAsync(
        Guid id,
        UpdateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Department", id);
        department.Update(new DepartmentName(command.Name), command.Description);
        await _repository.SaveChangesAsync(cancellationToken);
        return new DepartmentDto(department.Id, department.Name.Value, department.Description);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Department", id);
        _repository.Remove(department);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed record UpdateDepartmentCommand(string Name, string? Description);
