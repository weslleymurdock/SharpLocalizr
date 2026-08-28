using System.Text.Json;
using Localizr.Application.Localization.Abstractions;

namespace Localizr.Infrastructure.Localization.Resources;

/// <summary>Reads flat JSON localization resources into key/value entries.</summary>
public sealed class JsonResourceReader : IResourceReader
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow
    };

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using JsonDocument document = await JsonDocument.ParseAsync(
            stream,
            DocumentOptions,
            cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The localization resource must contain a JSON object at the root.");
        }

        Dictionary<string, string> resources = new(StringComparer.Ordinal);

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"The resource value for '{property.Name}' must be a JSON string.");
            }

            resources.Add(property.Name, property.Value.GetString() ?? string.Empty);
        }

        return resources;
    }
}
