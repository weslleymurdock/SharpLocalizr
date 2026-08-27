using Localizr.Application.Localization.Responses;

namespace Localizr.Application.Localization.Abstractions;

/// <summary>Provides provider-agnostic configuration and usage operations for localization providers.</summary>
public interface ILocalizationSettingsService
{
    /// <summary>Gets the current settings for a localization provider.</summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current provider settings, or <see langword="null"/> when the provider is not configured.</returns>
    Task<LocalizationProviderSettingsResponse?> GetProviderSettingsAsync(
        string provider,
        CancellationToken cancellationToken);

    /// <summary>Updates the settings for a localization provider.</summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="apiKey">The provider API key.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The updated provider settings with sensitive values masked.</returns>
    Task<LocalizationProviderSettingsResponse> UpdateProviderSettingsAsync(
        string provider,
        string apiKey,
        CancellationToken cancellationToken);

    /// <summary>Gets usage and remaining quota information for a localization provider.</summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The provider usage information.</returns>
    Task<LocalizationProviderUsageResponse> GetProviderUsageAsync(
        string provider,
        CancellationToken cancellationToken);
}
