using Localizr.Infrastructure.Persistence;
using Localizr.Application.Health.Abstractions;

namespace Localizr.Infrastructure.Health.Services;

public sealed class HealthService(LocalizrDbContext context) : IHealthService
{
    public async ValueTask<bool> CanConnectAsync(CancellationToken cancellationToken) => await context.Database.CanConnectAsync(cancellationToken);
    public async ValueTask<string> GetProviderNameAsync() => context.Database.ProviderName ?? "PostgreSQL";
}
