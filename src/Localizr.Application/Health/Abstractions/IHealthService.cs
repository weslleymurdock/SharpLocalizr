namespace Localizr.Application.Health.Abstractions;

public interface IHealthService
{
    ValueTask<bool> CanConnectAsync(CancellationToken cancellationToken);
    ValueTask<string> GetProviderNameAsync();
}
