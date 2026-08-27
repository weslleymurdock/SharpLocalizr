using System.Text;
using System.Text.Json;
using Localizr.Infrastructure.Localization.Resources;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for the JSON localization resource writer.</summary>
public sealed class JsonResourceWriterTests
{
    /// <summary>Verifies that entries are written as a valid flat JSON object.</summary>
    [Fact]
    public async Task WriteAsync_WhenResourcesAreValid_ShouldWriteJson()
    {
        Dictionary<string, string> resources = new(StringComparer.Ordinal)
        {
            ["Hello"] = "Olá",
            ["Goodbye"] = "Até logo"
        };
        await using MemoryStream stream = new();
        JsonResourceWriter writer = new();

        await writer.WriteAsync(resources, stream, CancellationToken.None);
        stream.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(stream);

        Assert.Equal("Olá", document.RootElement.GetProperty("Hello").GetString());
        Assert.Equal("Até logo", document.RootElement.GetProperty("Goodbye").GetString());
    }

    /// <summary>Verifies that an empty resource dictionary produces an empty JSON object.</summary>
    [Fact]
    public async Task WriteAsync_WhenResourcesAreEmpty_ShouldWriteEmptyObject()
    {
        await using MemoryStream stream = new();
        JsonResourceWriter writer = new();

        await writer.WriteAsync(new Dictionary<string, string>(), stream, CancellationToken.None);
        stream.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(stream);

        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        Assert.Empty(document.RootElement.EnumerateObject());
    }

    /// <summary>Verifies that special characters are serialized without losing their values.</summary>
    [Fact]
    public async Task WriteAsync_WhenValuesContainSpecialCharacters_ShouldPreserveValues()
    {
        const string value = "Line 1\n\"Line 2\"";
        await using MemoryStream stream = new();
        JsonResourceWriter writer = new();

        await writer.WriteAsync(
            new Dictionary<string, string> { ["Message"] = value },
            stream,
            CancellationToken.None);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8, leaveOpen: true);
        string json = await reader.ReadToEndAsync();
        using JsonDocument document = JsonDocument.Parse(json);

        Assert.Equal(value, document.RootElement.GetProperty("Message").GetString());
    }
}
