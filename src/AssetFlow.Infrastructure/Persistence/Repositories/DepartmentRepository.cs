using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

internal sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly AssetFlowDbContext _dbContext;

    public DepartmentRepository(AssetFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Departments.SingleOrDefaultAsync(
            department => department.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _dbContext.Departments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return departments.OrderBy(department => department.Name.Value).ToArray();
    }
}
