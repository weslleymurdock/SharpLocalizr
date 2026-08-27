namespace Localizr.Application.Localization.Requests;

/// <summary>Represents an HTTP request to translate a localization resource.</summary>
/// <param name="Resources">The source resource entries.</param>
/// <param name="Culture">The target culture name.</param>
public sealed record TranslateResourceRequest(
    IReadOnlyDictionary<string, string> Resources,
    string Culture);
