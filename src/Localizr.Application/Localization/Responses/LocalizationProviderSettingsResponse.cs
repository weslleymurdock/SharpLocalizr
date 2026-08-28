namespace Localizr.Application.Localization.Responses;

/// <summary>Represents provider configuration exposed to the application boundary.</summary>
/// <param name="Provider">The provider identifier.</param>
/// <param name="Configured">Whether the provider has an API key configured.</param>
/// <param name="MaskedApiKey">The API key represented in a masked form.</param>
public sealed record LocalizationProviderSettingsResponse(
    string Provider,
    bool Configured,
    string? MaskedApiKey);

/// <summary>Represents provider usage and quota information.</summary>
/// <param name="Provider">The provider identifier.</param>
/// <param name="Available">Whether authoritative usage information is available.</param>
/// <param name="UsedCharacters">The number of characters consumed in the reported period, when available.</param>
/// <param name="RemainingCharacters">The number of characters remaining in the reported period, when available.</param>
/// <param name="Message">A provider-specific explanation when usage information is unavailable.</param>
public sealed record LocalizationProviderUsageResponse(
    string Provider,
    bool Available,
    long? UsedCharacters,
    long? RemainingCharacters,
    string? Message);
