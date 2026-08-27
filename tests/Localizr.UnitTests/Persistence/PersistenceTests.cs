using Localizr.Infrastructure.Common.Repository;
using Localizr.Infrastructure.Identity.Models;
using Localizr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Localizr.UnitTests.Persistence;

/// <summary>Contains tests for persistence behavior shared by the application.</summary>
public sealed class PersistenceTests
{
    /// <summary>Verifies soft-delete filters and persistence metadata for added, modified, and deleted users.</summary>
    [Fact]
    public async Task DbContext_ShouldApplyMetadataAndSoftDeleteFilter()
    {
        await using LocalizrDbContext context = CreateContext();
        User user = new("user@example.com") { Email = "user@example.com" };
        await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(true, "creator", TestContext.Current.CancellationToken);
        Assert.Equal("creator", user.CreatedBy);
        Assert.NotEqual(default, user.CreatedAt);
        user.DisplayName = "Updated";
        await context.SaveChangesAsync(true, "updater", TestContext.Current.CancellationToken);
        Assert.Equal("updater", user.UpdatedBy);
        context.Users.Remove(user);
        await context.SaveChangesAsync(true, "deleter", TestContext.Current.CancellationToken);
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        Assert.Null(await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies the synchronous save overloads persist metadata without an explicit user identifier.</summary>
    [Fact]
    public void DbContext_SynchronousSaveOverloads_ShouldPersistChanges()
    {
        using LocalizrDbContext context = CreateContext();
        User user = new("sync@example.com") { Email = "sync@example.com" };
        context.Users.Add(user);
        Assert.Equal(1, context.SaveChanges(false));
        Assert.Equal(string.Empty, user.CreatedBy);
        user.DisplayName = "changed";
        Assert.Equal(1, context.SaveChanges(true));
        Assert.Equal(string.Empty, user.UpdatedBy);
        user.IsDeleted = false;
        context.Users.Remove(user);
        Assert.Equal(1, context.SaveChanges());
        Assert.True(user.IsDeleted);
    }

    /// <summary>Verifies the user-specific asynchronous save overload persists metadata.</summary>
    [Fact]
    public async Task DbContext_UserIdAsyncOverload_ShouldPersistMetadata()
    {
        await using LocalizrDbContext context = CreateContext();
        User user = new("async@example.com") { Email = "async@example.com" };
        await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync("async-user", TestContext.Current.CancellationToken);
        Assert.Equal("async-user", user.CreatedBy);
    }

    /// <summary>Verifies the default asynchronous save overload can persist an entity without an explicit user identifier.</summary>
    [Fact]
    public async Task DbContext_DefaultAsyncOverload_ShouldHandleUnchangedEntity()
    {
        await using LocalizrDbContext context = CreateContext();
        User user = new("unchanged@example.com") { Email = "unchanged@example.com" };
        await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        int result = await context.SaveChangesAsync(false, TestContext.Current.CancellationToken);
        Assert.Equal(0, result);
    }

    /// <summary>Verifies repository reads, writes, predicates, and cancellation-aware operations.</summary>
    [Fact]
    public async Task Repository_ShouldSupportCrudAndQueries()
    {
        await using LocalizrDbContext context = CreateContext();
        Repository<User> repository = new(context);
        User user = new("repository@example.com") { Email = "repository@example.com" };
        await repository.AddAsync(user, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync("repository-user", TestContext.Current.CancellationToken);
        User? byId = await repository.GetByIdAsync(user.Id, [], TestContext.Current.CancellationToken);
        User? tracked = await repository.GetTrackedByIdAsync(user.Id, [], TestContext.Current.CancellationToken);
        User? byPredicate = await repository.FirstOrDefaultAsync(x => x.Email == user.Email, [], TestContext.Current.CancellationToken);
        IReadOnlyList<User> all = await repository.ListAsync(null, [], TestContext.Current.CancellationToken);
        IReadOnlyList<User> filtered = await repository.ListAsync(x => x.Email == user.Email, [], TestContext.Current.CancellationToken);
        Assert.NotNull(byId);
        Assert.NotNull(tracked);
        Assert.NotNull(byPredicate);
        Assert.Single(all);
        Assert.Single(filtered);
        Assert.True(await repository.ExistsAsync(x => x.Email == user.Email, TestContext.Current.CancellationToken));
        Assert.False(await repository.ExistsAsync(x => x.Email == "missing@example.com", TestContext.Current.CancellationToken));
        tracked!.DisplayName = "updated";
        repository.Update(tracked);
        await context.SaveChangesAsync("repository-user", TestContext.Current.CancellationToken);
        Assert.Equal("updated", (await repository.GetByIdAsync(user.Id, [], TestContext.Current.CancellationToken))!.DisplayName);
        repository.Remove(tracked);
        await context.SaveChangesAsync("repository-user", TestContext.Current.CancellationToken);
        Assert.False(await repository.ExistsAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies repository range removal marks every supplied entity for deletion.</summary>
    [Fact]
    public async Task Repository_RemoveRange_ShouldRemoveAllEntities()
    {
        await using LocalizrDbContext context = CreateContext();
        Repository<User> repository = new(context);
        User first = new("first@example.com") { Email = "first@example.com" };
        User second = new("second@example.com") { Email = "second@example.com" };
        await repository.AddAsync(first, TestContext.Current.CancellationToken);
        await repository.AddAsync(second, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync("repository-user", TestContext.Current.CancellationToken);
        repository.RemoveRange([first, second]);
        await context.SaveChangesAsync("repository-user", TestContext.Current.CancellationToken);
        Assert.False(await repository.ExistsAsync(x => x.Id == first.Id, TestContext.Current.CancellationToken));
        Assert.False(await repository.ExistsAsync(x => x.Id == second.Id, TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies repository lookup methods return null when an identifier or predicate does not match.</summary>
    [Fact]
    public async Task Repository_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        await using LocalizrDbContext context = CreateContext();
        Repository<User> repository = new(context);
        Assert.Null(await repository.GetByIdAsync("missing", [], TestContext.Current.CancellationToken));
        Assert.Null(await repository.GetTrackedByIdAsync("missing", [], TestContext.Current.CancellationToken));
        Assert.Null(await repository.FirstOrDefaultAsync(x => x.Email == "missing@example.com", [], TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies repository list operations support an empty result.</summary>
    [Fact]
    public async Task Repository_ListAsync_WhenNoEntitiesMatch_ShouldReturnEmptyList()
    {
        await using LocalizrDbContext context = CreateContext();
        Repository<User> repository = new(context);
        IReadOnlyList<User> result = await repository.ListAsync(x => x.Email == "missing@example.com", [], TestContext.Current.CancellationToken);
        Assert.Empty(result);
    }

    /// <summary>Verifies domain entities preserve metadata supplied to their base constructor.</summary>
    [Fact]
    public void EntityBase_WhenValuesAreSupplied_ShouldPreserveThem()
    {
        DateTimeOffset created = DateTimeOffset.UtcNow.AddMinutes(-5);
        TestEntity entity = new("id", created);
        Assert.Equal("id", entity.Id);
        Assert.Equal(created, entity.CreatedAt);
        Assert.Equal(created, entity.UpdatedAt);
    }

    /// <summary>Verifies domain entities initialize generated metadata without requiring exact clock precision.</summary>
    [Fact]
    public void EntityBase_WhenDefaultsAreUsed_ShouldInitializeMetadata()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        TestEntity entity = new();
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.NotEmpty(entity.Id);
        Assert.InRange(entity.CreatedAt, before, after);
        Assert.Equal(string.Empty, entity.CreatedBy);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
        Assert.Equal(string.Empty, entity.UpdatedBy);
    }

    private static LocalizrDbContext CreateContext()
    {
        DbContextOptions<LocalizrDbContext> options = new DbContextOptionsBuilder<LocalizrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LocalizrDbContext(options);
    }
}

/// <summary>Provides a concrete entity for testing the domain base class.</summary>
internal sealed class TestEntity(string id = "", DateTimeOffset? createdAt = null) : Localizr.Domain.Common.EntityBase(id, createdAt), Localizr.Domain.Common.ISoftDeletable
{
    /// <summary>Gets or sets whether the entity is deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the deletion timestamp.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
