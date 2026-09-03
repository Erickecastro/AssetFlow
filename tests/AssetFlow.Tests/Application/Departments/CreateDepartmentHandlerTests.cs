using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Application.Departments.CreateDepartment;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Tests.Application.Departments;

public sealed class CreateDepartmentHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateAndPersistDepartment()
    {
        var repository = new DepartmentRepositorySpy();
        var handler = new CreateDepartmentHandler(repository);

        var result = await handler.HandleAsync(
            new CreateDepartmentCommand("  Tecnologia  ", "  Segundo andar.  "));

        Assert.NotNull(repository.AddedDepartment);
        Assert.Equal(repository.AddedDepartment.Id, result.Id);
        Assert.Equal("Tecnologia", result.Name);
        Assert.Equal("Segundo andar.", result.Description);
    }

    [Fact]
    public async Task HandleAsync_ShouldForwardCancellationToken()
    {
        var repository = new DepartmentRepositorySpy();
        var handler = new CreateDepartmentHandler(repository);
        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new CreateDepartmentCommand("Tecnologia"),
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            repository.ReceivedCancellationToken);
    }

    private sealed class DepartmentRepositorySpy : IDepartmentRepository
    {
        public Department? AddedDepartment { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(
            Department department,
            CancellationToken cancellationToken = default)
        {
            AddedDepartment = department;
            ReceivedCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<Department?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Department?>(null);
        }

        public Task<IReadOnlyList<Department>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Department>>([]);
        }
    }
}
