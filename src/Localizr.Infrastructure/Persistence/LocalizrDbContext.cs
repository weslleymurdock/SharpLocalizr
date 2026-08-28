using System.Linq.Expressions;
using Localizr.Domain.Common;
using Localizr.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Localizr.Infrastructure.Persistence;

/// <summary>Represents the Localizr database context.</summary>
/// <param name="options">The context options.</param>
public sealed class LocalizrDbContext(DbContextOptions<LocalizrDbContext> options)
    : IdentityDbContext<User, Role, string>(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "entity");
            MemberExpression property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            LambdaExpression filter = Expression.Lambda(Expression.Not(property), parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        builder.ApplyConfigurationsFromAssembly(typeof(LocalizrDbContext).Assembly);
    }

    /// <summary>Saves changes using an explicit user identifier.</summary>
    /// <param name="acceptAllChangesOnSuccess">Whether changes are accepted after saving.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <returns>The number of state entries written.</returns>
    public int SaveChanges(bool acceptAllChangesOnSuccess, string userId)
    {
        ApplyEntityIdentifiers(userId);
        ApplySoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChanges();
    }

    /// <summary>Saves changes asynchronously using an explicit user identifier.</summary>
    /// <param name="acceptAllChangesOnSuccess">Whether changes are accepted after saving.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the number of state entries written.</returns>
    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, string userId, CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers(userId);
        ApplySoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>Saves changes asynchronously using an explicit user identifier.</summary>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the number of state entries written.</returns>
    public Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers(userId);
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyEntityIdentifiers(string? userId = null)
    {
        foreach (EntityEntry<IEntityBase> entry in ChangeTracker.Entries<IEntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    entry.Entity.CreatedBy = userId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    entry.Entity.UpdatedBy = userId;
                }
            }
            else
            {
                continue;
            }
        }
    }

    private void ApplySoftDelete()
    {
        foreach (EntityEntry<ISoftDeletable> entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
