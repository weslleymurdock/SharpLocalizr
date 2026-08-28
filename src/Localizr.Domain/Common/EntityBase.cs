namespace Localizr.Domain.Common;

/// <summary>
/// Provides common persistence metadata.
/// </summary>
public abstract class EntityBase : IEntityBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBase"/> class.
    /// </summary>
    protected EntityBase()
    {
        Id = Guid.CreateVersion7().ToString();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBase"/> class with the specified identifier and creation timestamp.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="createdAt"></param>
    public EntityBase(string id = "", DateTimeOffset? createdAt = null) : this()
    {
        Id = id == string.Empty ? Guid.CreateVersion7().ToString() : id;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; }

    /// <inheritdoc />
    public string UpdatedBy { get; set; } =
        string.Empty;
}
