using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Localizr.Application.Localization.Abstractions;
using Localizr.Infrastructure.Localization.Exceptions;
using Localizr.Infrastructure.Localization.Options;
using Microsoft.Extensions.Options;

namespace Localizr.Infrastructure.Localization.Services;

/// <summary>Translates localization resource values through the Google Cloud Translation Basic API.</summary>
/// <param name="httpClient">The HTTP client used to communicate with Google Cloud Translation.</param>
/// <param name="options">The Google Cloud Translation configuration.</param>
public sealed class TranslatorService(
    HttpClient httpClient,
    IOptions<GoogleTranslateOptions> options) : ITranslatorService
{
    private const int MaximumEntriesPerRequest = 128;
    private readonly GoogleTranslateOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> TranslateToCultureAsync(
        IReadOnlyDictionary<string, string> resources,
        string culture,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);

        if (resources.Count == 0)
        {
            return [];
        }

        ValidateConfiguration();

        Dictionary<string, string> translatedResources = new(resources.Count, StringComparer.Ordinal);
        List<KeyValuePair<string, string>> pendingEntries = resources
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .ToList();

        foreach (KeyValuePair<string, string> entry in resources.Where(entry => string.IsNullOrWhiteSpace(entry.Value)))
        {
            translatedResources[entry.Key] = entry.Value;
        }

        foreach (KeyValuePair<string, string>[] batch in pendingEntries.Chunk(MaximumEntriesPerRequest))
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<string> translations = await TranslateBatchAsync(
                batch.Select(entry => entry.Value).ToArray(),
                culture,
                cancellationToken);

            if (translations.Count != batch.Length)
            {
                throw new TranslationProviderException(
                    "The translation provider returned a different number of translations than requested.");
            }

            for (int index = 0; index < batch.Length; index++)
            {
                translatedResources[batch[index].Key] = translations[index];
            }
        }

        return translatedResources;
    }

    private async Task<IReadOnlyList<string>> TranslateBatchAsync(
        IReadOnlyList<string> values,
        string culture,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            "language/translate/v2");

        request.Headers.Add("X-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new GoogleTranslateRequest(
            values,
            culture,
            "text"));

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new TranslationProviderException(
                $"Google Cloud Translation returned HTTP {(int)response.StatusCode}: {responseContent}",
                (int)response.StatusCode);
        }

        GoogleTranslateResponse? result = JsonSerializer.Deserialize<GoogleTranslateResponse>(responseContent);
        if (result?.Data?.Translations is null)
        {
            throw new TranslationProviderException(
                "Google Cloud Translation returned an invalid response payload.");
        }

        return result.Data.Translations
            .Select(translation => translation.TranslatedText)
            .ToArray();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                $"Google translation API key is not configured. Configure '{GoogleTranslateOptions.SectionName}:ApiKey'.");
        }

        if (!Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out Uri? endpoint))
        {
            throw new InvalidOperationException(
                $"Google translation endpoint '{_options.Endpoint}' is not a valid absolute URI.");
        }

        if (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The Google translation endpoint must use HTTPS.");
        }
    }

    private sealed record GoogleTranslateRequest(
        [property: JsonPropertyName("q")] IReadOnlyList<string> Queries,
        [property: JsonPropertyName("target")] string Target,
        [property: JsonPropertyName("format")] string Format);

    private sealed record GoogleTranslateResponse(
        [property: JsonPropertyName("data")] GoogleTranslateData? Data);

    private sealed record GoogleTranslateData(
        [property: JsonPropertyName("translations")] IReadOnlyList<GoogleTranslation>? Translations);

    private sealed record GoogleTranslation(
        [property: JsonPropertyName("translatedText")] string TranslatedText);
}
