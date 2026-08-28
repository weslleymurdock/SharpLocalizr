using System.Text.Json;
using Localizr.Infrastructure.Localization.Resources;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains integration-style tests for JSON resource round trips.</summary>
public sealed class JsonResourceRoundTripTests
{
    /// <summary>Verifies that writing and reading a resource preserves keys and values.</summary>
    [Fact]
    public async Task WriteAndReadAsync_ShouldPreserveResources()
    {
        Dictionary<string, string> source = new(StringComparer.Ordinal)
        {
            ["Welcome.Title"] = "Welcome",
            ["Welcome.Message"] = "Hello, world!",
            ["Empty"] = string.Empty
        };

        await using MemoryStream stream = new();
        JsonResourceWriter writer = new();
        JsonResourceReader reader = new();

        await writer.WriteAsync(source, stream, CancellationToken.None);
        stream.Position = 0;
        IReadOnlyDictionary<string, string> result = await reader.ReadAsync(stream, CancellationToken.None);

        Assert.Equal(source, result);
    }

    /// <summary>Verifies that a canceled read observes the supplied cancellation token.</summary>
    [Fact]
    public async Task ReadAsync_WhenCanceled_ShouldThrowOperationCanceledException()
    {
        await using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, new Dictionary<string, string> { ["Key"] = "Value" }, new JsonSerializerOptions(){ WriteIndented = true }, TestContext.Current.CancellationToken);
        stream.Position = 0;

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        JsonResourceReader reader = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            reader.ReadAsync(stream, cancellationTokenSource.Token));
    }

    /// <summary>Verifies that a canceled write observes the supplied cancellation token.</summary>
    [Fact]
    public async Task WriteAsync_WhenCanceled_ShouldThrowOperationCanceledException()
    {
        await using MemoryStream stream = new();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        JsonResourceWriter writer = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            writer.WriteAsync(
                new Dictionary<string, string> { ["Key"] = "Value" },
                stream,
                cancellationTokenSource.Token));
    }
}
