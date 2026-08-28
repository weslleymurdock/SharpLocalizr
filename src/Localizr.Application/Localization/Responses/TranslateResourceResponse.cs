namespace Localizr.Application.Localization.Responses;

/// <summary>Represents a translated localization resource.</summary>
/// <param name="Resources">The translated resource entries.</param>
/// <param name="Culture">The target culture used for translation.</param>
public sealed record TranslateResourceResponse(
    IReadOnlyDictionary<string, string> Resources,
    string Culture);
