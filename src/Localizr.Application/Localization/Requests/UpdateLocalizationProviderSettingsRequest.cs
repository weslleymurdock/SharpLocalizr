namespace Localizr.Application.Localization.Requests;

/// <summary>Represents an HTTP request to update a localization provider API key.</summary>
/// <param name="Provider">The provider identifier.</param>
/// <param name="ApiKey">The provider API key.</param>
public sealed record UpdateLocalizationProviderSettingsRequest(
    string Provider,
    string ApiKey);
