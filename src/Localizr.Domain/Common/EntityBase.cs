namespace Localizr.Domain.Common;

/// <summary>
/// Provides common persistence metadata.
/// </summary>
public abstract class EntityBase(string id = "",
    DateTimeOffset? createdAt = null) : IEntityBase
{
    private readonly DateTimeOffset initialTimestamp =
        createdAt ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string Id { get; set; } = id == string.Empty ?
        Guid.CreateVersion7().ToString() : id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } =
        initialTimestamp;

    /// <inheritdoc />
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; } =
        initialTimestamp;

    /// <inheritdoc />
    public string UpdatedBy { get; set; } =
        string.Empty;
}
