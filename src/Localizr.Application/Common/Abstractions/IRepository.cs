using System.Linq.Expressions;
using Localizr.Domain.Common;

namespace Localizr.Application.Common.Abstractions;

/// <summary>
/// Defines persistence operations for entities.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type.
/// </typeparam>
public interface IRepository<TEntity>
    where TEntity : class, IEntityBase
{
    /// <summary>
    /// Gets an entity without tracking.
    /// </summary>
    /// <param name="id">
    /// The entity identifier.
    /// </param>
    /// <param name="includes">
    /// Navigation properties to include.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The entity when found.
    /// </returns>
    Task<TEntity?> GetByIdAsync(
        string id,
        Expression<Func<TEntity, object?>>[] includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a tracked entity.
    /// </summary>
    /// <param name="id">
    /// The entity identifier.
    /// </param>
    /// <param name="includes">
    /// Navigation properties to include.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The entity when found.
    /// </returns>
    Task<TEntity?> GetTrackedByIdAsync(
        string id,
        Expression<Func<TEntity, object?>>[] includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Finds the first matching entity.
    /// </summary>
    /// <param name="predicate">
    /// The query predicate.
    /// </param>
    /// <param name="includes">
    /// Navigation properties to include.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The first matching entity.
    /// </returns>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object?>>[] includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists entities matching a predicate.
    /// </summary>
    /// <param name="predicate">
    /// The optional query predicate.
    /// </param>
    /// <param name="includes">
    /// Navigation properties to include.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The matching entities.
    /// </returns>
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object?>>[] includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether an entity matches.
    /// </summary>
    /// <param name="predicate">
    /// The query predicate.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when found.
    /// </returns>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds an entity to the unit of work.
    /// </summary>
    /// <param name="entity">
    /// The entity to add.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates an entity in the unit of work.
    /// </summary>
    /// <param name="entity">
    /// The entity to update.
    /// </param>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the unit of work.
    /// </summary>
    /// <param name="entity">
    /// The entity to remove.
    /// </param>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes entities from the unit of work.
    /// </summary>
    /// <param name="entities">
    /// The entities to remove.
    /// </param>
    void RemoveRange(IEnumerable<TEntity> entities);
}
