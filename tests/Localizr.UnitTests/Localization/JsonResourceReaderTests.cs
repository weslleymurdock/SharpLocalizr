using System.Text;
using System.Text.Json;
using Localizr.Infrastructure.Localization.Resources;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for the JSON localization resource reader.</summary>
public sealed class JsonResourceReaderTests
{
    /// <summary>Verifies that a flat JSON object is read into key/value entries.</summary>
    [Fact]
    public async Task ReadAsync_WhenJsonIsValid_ShouldReadEntries()
    {
        await using MemoryStream stream = CreateStream("{\"Hello\":\"Hello world\",\"Bye\":\"Goodbye\"}");
        JsonResourceReader reader = new();

        IReadOnlyDictionary<string, string> result = await reader.ReadAsync(stream, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Hello world", result["Hello"]);
        Assert.Equal("Goodbye", result["Bye"]);
    }

    /// <summary>Verifies that an empty JSON object is accepted.</summary>
    [Fact]
    public async Task ReadAsync_WhenJsonIsEmptyObject_ShouldReturnEmptyDictionary()
    {
        await using MemoryStream stream = CreateStream("{}");
        JsonResourceReader reader = new();

        IReadOnlyDictionary<string, string> result = await reader.ReadAsync(stream, CancellationToken.None);

        Assert.Empty(result);
    }

    /// <summary>Verifies that non-string resource values are rejected.</summary>
    [Fact]
    public async Task ReadAsync_WhenValueIsNotString_ShouldThrowJsonException()
    {
        await using MemoryStream stream = CreateStream("{\"Count\":1}");
        JsonResourceReader reader = new();

        await Assert.ThrowsAsync<JsonException>(() => reader.ReadAsync(stream, CancellationToken.None));
    }

    /// <summary>Verifies that a non-object JSON root is rejected.</summary>
    [Fact]
    public async Task ReadAsync_WhenRootIsNotObject_ShouldThrowJsonException()
    {
        await using MemoryStream stream = CreateStream("[\"value\"]");
        JsonResourceReader reader = new();

        await Assert.ThrowsAsync<JsonException>(() => reader.ReadAsync(stream, CancellationToken.None));
    }

    private static MemoryStream CreateStream(string content) =>
        new(Encoding.UTF8.GetBytes(content));
}
