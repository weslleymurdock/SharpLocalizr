using Localizr.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Localizr.Infrastructure.Identity.Models;

/// <summary>
/// Represents an Localizr application role.
/// </summary>
public class Role(
    string? name = null)
    : IdentityRole<string>(
        name ?? string.Empty),
      IEntityBase,
      ISoftDeletable
{
    /// <summary>
    /// Gets or sets the identity identifier.
    /// </summary>
    public new string Id { get; set; } =
        Guid.CreateVersion7().ToString();

    /// <summary>
    /// Gets or sets whether the role is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the last updater identifier.
    /// </summary>
    public string UpdatedBy { get; set; } =
        string.Empty;
}
