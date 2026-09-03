using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Infrastructure.Persistence;
using AssetFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AssetFlow")
            ?? throw new InvalidOperationException(
                "Connection string 'AssetFlow' is not configured.");

        services.AddDbContext<AssetFlowDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        return services;
    }
}
