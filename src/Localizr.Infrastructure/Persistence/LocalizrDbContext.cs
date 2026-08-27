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
    protected override void OnModelCreating(
        ModelBuilder builder)
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

    /// <summary>
    ///     Saves all changes made in this context to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method will automatically call <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" />
    ///         to discover any changes to entity instances before saving to the underlying database. This can be disabled via
    ///         <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" />
    ///     is called after the changes have been sent successfully to the database.
    /// </param>
    /// <param name="userId">
    ///     Sets the id of the user that have performed the action on database.
    /// </param>
    /// <returns>
    ///     The number of state entries written to the database.
    /// </returns>
    /// <exception cref="DbUpdateException">
    ///     An error is encountered while saving to the database.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    ///     A concurrency violation is encountered while saving to the database.
    ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
    ///     This is usually because the data in the database has been modified since it was loaded into memory.
    /// </exception>
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
        return base.SaveChanges(
            acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChanges();
    }

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
    
    /// <summary>
    ///     Saves all changes made in this context to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method will automatically call <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" />
    ///         to discover any changes to entity instances before saving to the underlying database. This can be disabled via
    ///         <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more
    ///         information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="userId">The id of user that requested save changes on database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the
    ///     number of state entries written to the database.
    /// </returns>
    /// <exception cref="DbUpdateException">
    ///     An error is encountered while saving to the database.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    ///     A concurrency violation is encountered while saving to the database.
    ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
    ///     This is usually because the data in the database has been modified since it was loaded into memory.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
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

    private void ApplyEntityIdentifiers(string userId = null!)
    {
        foreach (EntityEntry<IEntityBase> entry in ChangeTracker.Entries<IEntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is User || entry.Entity is Role)
                {
                    entry.Entity.Id = Guid.CreateVersion7().ToString();
                }
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = userId;
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
