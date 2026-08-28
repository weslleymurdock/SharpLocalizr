using System.Text.Json;
using Localizr.Application.Localization.Abstractions;

namespace Localizr.Infrastructure.Localization.Resources;

/// <summary>Writes localization key/value entries as a flat JSON object.</summary>
public sealed class JsonResourceWriter : IResourceWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public async Task WriteAsync(
        IReadOnlyDictionary<string, string> resources,
        Stream stream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(stream);

        await JsonSerializer.SerializeAsync(
            stream,
            resources,
            SerializerOptions,
            cancellationToken);
    }
}
