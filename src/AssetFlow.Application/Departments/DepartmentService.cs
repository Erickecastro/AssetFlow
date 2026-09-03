using AssetFlow.Application.Abstractions.Persistence;

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
}
