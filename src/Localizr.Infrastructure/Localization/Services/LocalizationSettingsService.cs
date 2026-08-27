using System.Text.Json;
using System.Text.Json.Nodes;
using Localizr.Application.Localization.Abstractions;
using Localizr.Application.Localization.Responses;
using Localizr.Infrastructure.Localization.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Localizr.Infrastructure.Localization.Services;

/// <summary>Manages localization provider configuration and usage information.</summary>
/// <param name="configuration">The application configuration.</param>
/// <param name="environment">The host environment.</param>
/// <param name="googleOptions">The monitored Google Translate options.</param>
/// <param name="usageTracker">The current-instance Google Translate usage tracker.</param>
public sealed class LocalizationSettingsService(
    IConfiguration configuration,
    IHostEnvironment environment,
    IOptionsMonitor<GoogleTranslateOptions> googleOptions,
    GoogleTranslateUsageTracker usageTracker) : ILocalizationSettingsService
{
    private const string GoogleProvider = "google";
    private const long MonthlyFreeCharacters = 500_000;
    private readonly SemaphoreSlim writeLock = new(1, 1);

    /// <inheritdoc />
    public Task<LocalizationProviderSettingsResponse?> GetProviderSettingsAsync(
        string provider,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsGoogle(provider))
        {
            return Task.FromResult<LocalizationProviderSettingsResponse?>(null);
        }

        string apiKey = googleOptions.CurrentValue.ApiKey;
        return Task.FromResult<LocalizationProviderSettingsResponse?>(
            new LocalizationProviderSettingsResponse(
                GoogleProvider,
                !string.IsNullOrWhiteSpace(apiKey),
                MaskApiKey(apiKey)));
    }

    /// <inheritdoc />
    public async Task<LocalizationProviderSettingsResponse> UpdateProviderSettingsAsync(
        string provider,
        string apiKey,
        CancellationToken cancellationToken)
    {
        if (!IsGoogle(provider))
        {
            throw new ArgumentException("The requested localization provider is not supported.", nameof(provider));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        cancellationToken.ThrowIfCancellationRequested();

        await writeLock.WaitAsync(cancellationToken);
        try
        {
            string configurationPath = Path.Combine(environment.ContentRootPath, "appsettings.json");
            string json = await File.ReadAllTextAsync(configurationPath, cancellationToken);
            JsonNode root = JsonNode.Parse(json)
                ?? throw new InvalidOperationException("The application configuration file is invalid.");

            JsonObject rootObject = root.AsObject();
            JsonObject googleObject = rootObject[GoogleTranslateOptions.SectionName]?.AsObject()
                ?? new JsonObject();
            googleObject["ApiKey"] = apiKey;
            rootObject[GoogleTranslateOptions.SectionName] = googleObject;

            string updatedJson = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configurationPath, updatedJson, cancellationToken);

            if (configuration is IConfigurationRoot configurationRoot)
            {
                configurationRoot.Reload();
            }
        }
        finally
        {
            writeLock.Release();
        }

        return new LocalizationProviderSettingsResponse(
            GoogleProvider,
            true,
            MaskApiKey(apiKey));
    }

    /// <inheritdoc />
    public Task<LocalizationProviderUsageResponse> GetProviderUsageAsync(
        string provider,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsGoogle(provider))
        {
            return Task.FromResult(new LocalizationProviderUsageResponse(
                provider,
                false,
                null,
                null,
                "The requested localization provider is not supported."));
        }

        long usedCharacters = usageTracker.GetUsedCharacters();
        long remainingCharacters = Math.Max(0, MonthlyFreeCharacters - usedCharacters);

        return Task.FromResult(new LocalizationProviderUsageResponse(
            GoogleProvider,
            true,
            usedCharacters,
            remainingCharacters,
            "Usage is tracked locally by this application instance. Google Cloud billing and project-wide quota usage require authenticated Cloud Billing or Service Usage access and are not exposed by the API-key-only Translation Basic API."));
    }

    private static bool IsGoogle(string provider)
    {
        return string.Equals(provider, GoogleProvider, StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "GoogleTranslate", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "Google Cloud Translation", StringComparison.OrdinalIgnoreCase);
    }

    private static string? MaskApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        return apiKey.Length <= 8
            ? "********"
            : $"{apiKey[..4]}...{apiKey[^4..]}";
    }
}
