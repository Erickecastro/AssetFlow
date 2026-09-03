using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions.Persistence;

public interface IDepartmentRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default);
}
