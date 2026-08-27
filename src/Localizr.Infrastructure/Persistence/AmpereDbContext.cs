using System.Linq.Expressions;
using Localizr.Domain.Common;
using Localizr.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Localizr.Infrastructure.Persistence;

/// <summary>Represents the Localizr database context.</summary>
public sealed class LocalizrDbContext(
    DbContextOptions<LocalizrDbContext> options)
    : IdentityDbContext<User, Role, string>(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (IMutableEntityType entityType
            in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(
                entityType.ClrType))
            {
                continue;
            }

            ParameterExpression parameter =
                Expression.Parameter(
                    entityType.ClrType,
                    "entity");
            MemberExpression property =
                Expression.Property(
                    parameter,
                    nameof(ISoftDeletable.IsDeleted));
            LambdaExpression filter =
                Expression.Lambda(
                    Expression.Not(property),
                    parameter);

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }

        builder.ApplyConfigurationsFromAssembly(
            typeof(LocalizrDbContext).Assembly);
    }

    /// <inheritdoc />
    public override int SaveChanges(
        bool acceptAllChangesOnSuccess)
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

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(
            cancellationToken);
    }

    private void ApplyEntityIdentifiers()
    {
        foreach (EntityEntry<IEntityBase> entry
            in ChangeTracker.Entries<IEntityBase>())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            if (entry.Entity is User
                || entry.Entity is Role)
            {
                entry.Entity.Id =
                    Guid.CreateVersion7().ToString();
            }
        }
    }

    private void ApplySoftDelete()
    {
        foreach (EntityEntry<ISoftDeletable> entry
            in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt =
                DateTimeOffset.UtcNow;
        }
    }
}
