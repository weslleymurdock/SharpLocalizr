using Localizr.Application.Common.Abstractions;
using Localizr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Localizr.Infrastructure.Common.UnitOfWork;

/// <summary>
/// Coordinates EF Core persistence transactions.
/// </summary>
public sealed class UnitOfWork(
    LocalizrDbContext dbContext) : IUnitOfWork,
    IAsyncDisposable
{
    private IDbContextTransaction?
        transaction;

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            return;
        }

        transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken)
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.RollbackAsync();
        await DisposeTransactionAsync();
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.DisposeAsync();
        transaction = null;
    }
}
