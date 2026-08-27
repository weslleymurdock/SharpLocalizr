namespace Localizr.Domain.Common;

/// <summary>
/// Provides common persistence metadata.
/// </summary>
public abstract class EntityBase(string id = "",
    DateTimeOffset? createdAt = null) : IEntityBase
{
    /// <inheritdoc />
    public string Id { get; set; } = id == string.Empty ?
        Guid.CreateVersion7().ToString() : id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } =
        createdAt ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; } =
        createdAt ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string UpdatedBy { get; set; } =
        string.Empty;
}
