using Localizr.Application.Health.Abstractions;
using Localizr.Application.Health.Handlers;
using Localizr.Application.Health.Queries;
using Localizr.Infrastructure.Health.Services;
using Localizr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Localizr.UnitTests.Health;

/// <summary>Contains tests for application health queries, handlers, and infrastructure checks.</summary>
public sealed class HealthTests
{
    /// <summary>Verifies a successful database connectivity check is returned by the handler.</summary>
    [Fact]
    public async Task CanConnectAsync_WhenServiceIsHealthy_ShouldReturnSuccess()
    {
        IHealthService service = Substitute.For<IHealthService>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        service.CanConnectAsync(cancellationToken).Returns(new ValueTask<bool>(true));

        var result = await new HealthHandlers(service).Handle(new CanConnectAsyncQuery(), cancellationToken);

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        Assert.Empty(result.Errors);
        await service.Received(1).CanConnectAsync(cancellationToken);
    }

    /// <summary>Verifies a failed database connectivity check is represented as a failure response.</summary>
    [Fact]
    public async Task CanConnectAsync_WhenServiceIsUnhealthy_ShouldReturnFailure()
    {
        IHealthService service = Substitute.For<IHealthService>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        service.CanConnectAsync(cancellationToken).Returns(new ValueTask<bool>(false));

        var result = await new HealthHandlers(service).Handle(new CanConnectAsyncQuery(), cancellationToken);

        Assert.False(result.Succeeded);
        Assert.False(result.Data);
        Assert.Empty(result.Errors);
        await service.Received(1).CanConnectAsync(cancellationToken);
    }

    /// <summary>Verifies the provider-name query delegates to the health service and returns its value.</summary>
    [Fact]
    public async Task GetProviderNameAsync_ShouldReturnServiceProviderName()
    {
        IHealthService service = Substitute.For<IHealthService>();
        service.GetProviderNameAsync().Returns(new ValueTask<string>("Microsoft.EntityFrameworkCore.InMemory"));

        var result = await new HealthHandlers(service).Handle(new GetProviderNameQuery(), TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", result.Data);
        Assert.Empty(result.Errors);
        await service.Received(1).GetProviderNameAsync();
    }

    /// <summary>Verifies the infrastructure health service reports connectivity for a configured database.</summary>
    [Fact]
    public async Task CanConnectAsync_WhenDatabaseIsConfigured_ShouldReturnTrue()
    {
        await using LocalizrDbContext context = CreateContext();
        HealthService service = new(context);

        bool result = await service.CanConnectAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    /// <summary>Verifies the infrastructure health service returns the configured database provider name.</summary>
    [Fact]
    public async Task GetProviderNameAsync_WhenProviderNameExists_ShouldReturnProviderName()
    {
        await using LocalizrDbContext context = CreateContext();
        HealthService service = new(context);

        string result = await service.GetProviderNameAsync();

        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", result);
    }

    private static LocalizrDbContext CreateContext()
    {
        DbContextOptions<LocalizrDbContext> options = new DbContextOptionsBuilder<LocalizrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LocalizrDbContext(options);
    }
}
