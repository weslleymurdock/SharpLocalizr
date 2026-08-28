using Localizr.Infrastructure.Identity.Models;

namespace Localizr.UnitTests.Identity;

/// <summary>Contains tests for identity model initialization and metadata.</summary>
public sealed class IdentityModelsTests
{
    /// <summary>Verifies user defaults and custom username values.</summary>
    [Fact]
    public void User_ShouldInitializeDefaults()
    {
        User user = new("user@example.com");

        Assert.NotEmpty(user.Id);
        Assert.Equal("user@example.com", user.UserName);
        Assert.Equal(string.Empty, user.DisplayName);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.SurName);
        Assert.False(user.IsDeleted);
        Assert.NotEqual(default, user.CreatedAt);
        Assert.NotEqual(default, user.UpdatedAt);
    }

    /// <summary>Verifies a user without a username receives the default empty username.</summary>
    [Fact]
    public void User_WhenUsernameIsNull_ShouldUseEmptyUsername()
    {
        User user = new();

        Assert.Equal(string.Empty, user.UserName);
    }

    /// <summary>Verifies role defaults and custom role names.</summary>
    [Fact]
    public void Role_ShouldInitializeDefaults()
    {
        Role role = new("Administrator");

        Assert.NotEmpty(role.Id);
        Assert.Equal("Administrator", role.Name);
        Assert.False(role.IsDeleted);
        Assert.NotEqual(default, role.CreatedAt);
        Assert.NotEqual(default, role.UpdatedAt);
    }

    /// <summary>Verifies a role without a name receives the default empty name.</summary>
    [Fact]
    public void Role_WhenNameIsNull_ShouldUseEmptyName()
    {
        Role role = new();

        Assert.Equal(string.Empty, role.Name);
    }
}
