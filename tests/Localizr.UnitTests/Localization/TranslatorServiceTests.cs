using System.Net;
using System.Net.Http.Json;
using Localizr.Infrastructure.Localization.Exceptions;
using Localizr.Infrastructure.Localization.Options;
using Localizr.Infrastructure.Localization.Services;
using Microsoft.Extensions.Options;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for the Google translation service.</summary>
public sealed class TranslatorServiceTests
{
    /// <summary>Verifies that translated values are mapped to their original resource keys.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_ShouldPreserveKeys()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá", "Mundo"]));
        TranslatorService service = CreateService(client);
        Dictionary<string, string> resources = new()
        {
            ["Greeting"] = "Hello",
            ["Description"] = "World"
        };

        Dictionary<string, string> result = await service.TranslateToCultureAsync(resources, "pt-BR", CancellationToken.None);

        Assert.Equal("Olá", result["Greeting"]);
        Assert.Equal("Mundo", result["Description"]);
        Assert.Equal(resources.Keys.Order(), result.Keys.Order());
    }

    /// <summary>Verifies that empty values are preserved without sending them to the provider.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenValueIsEmpty_ShouldPreserveValue()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"]));
        TranslatorService service = CreateService(client);
        Dictionary<string, string> resources = new()
        {
            ["Greeting"] = "Hello",
            ["Empty"] = string.Empty
        };

        Dictionary<string, string> result = await service.TranslateToCultureAsync(resources, "pt-BR", CancellationToken.None);

        Assert.Equal(string.Empty, result["Empty"]);
        Assert.Equal("Olá", result["Greeting"]);
    }

    /// <summary>Verifies that an empty resource set does not invoke the provider.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenResourcesAreEmpty_ShouldReturnEmptyResult()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler([]));
        TranslatorService service = CreateService(client);

        Dictionary<string, string> result = await service.TranslateToCultureAsync(
            new Dictionary<string, string>(),
            "pt-BR",
            CancellationToken.None);

        Assert.Empty(result);
    }

    /// <summary>Verifies that a missing API key is rejected before contacting the provider.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenApiKeyIsMissing_ShouldThrowInvalidOperationException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"]));
        TranslatorService service = CreateService(client, apiKey: string.Empty);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.TranslateToCultureAsync(new Dictionary<string, string> { ["Greeting"] = "Hello" }, "pt-BR", CancellationToken.None));

        Assert.Contains("API key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies that an invalid endpoint is rejected before contacting the provider.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenEndpointIsInvalid_ShouldThrowInvalidOperationException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"]));
        TranslatorService service = CreateService(client, endpoint: "not-a-uri");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.TranslateToCultureAsync(new Dictionary<string, string> { ["Greeting"] = "Hello" }, "pt-BR", CancellationToken.None));

        Assert.Contains("valid absolute URI", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Verifies that a non-HTTPS endpoint is rejected.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenEndpointIsNotHttps_ShouldThrowInvalidOperationException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"]));
        TranslatorService service = CreateService(client, endpoint: "http://translation.googleapis.com/");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.TranslateToCultureAsync(new Dictionary<string, string> { ["Greeting"] = "Hello" }, "pt-BR", CancellationToken.None));

        Assert.Contains("HTTPS", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Verifies that a provider response with an unexpected number of translations is rejected.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenProviderReturnsUnexpectedCount_ShouldThrowTranslationProviderException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"]));
        TranslatorService service = CreateService(client);
        Dictionary<string, string> resources = new()
        {
            ["Greeting"] = "Hello",
            ["Description"] = "World"
        };

        TranslationProviderException exception = await Assert.ThrowsAsync<TranslationProviderException>(() =>
            service.TranslateToCultureAsync(resources, "pt-BR", CancellationToken.None));

        Assert.Contains("different number", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies that a malformed provider payload is rejected.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenProviderReturnsMalformedPayload_ShouldThrowTranslationProviderException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler([], payload: new { unexpected = true }));
        TranslatorService service = CreateService(client);

        TranslationProviderException exception = await Assert.ThrowsAsync<TranslationProviderException>(() =>
            service.TranslateToCultureAsync(new Dictionary<string, string> { ["Greeting"] = "Hello" }, "pt-BR", CancellationToken.None));

        Assert.Contains("invalid response payload", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verifies that a provider HTTP failure is exposed as a provider exception.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenProviderFails_ShouldThrowTranslationProviderException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler([], HttpStatusCode.BadGateway));
        TranslatorService service = CreateService(client);
        Dictionary<string, string> resources = new() { ["Greeting"] = "Hello" };

        TranslationProviderException exception = await Assert.ThrowsAsync<TranslationProviderException>(() =>
            service.TranslateToCultureAsync(resources, "pt-BR", CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.BadGateway, exception.StatusCode);
    }

    /// <summary>Verifies that cancellation is propagated to the HTTP operation.</summary>
    [Fact]
    public async Task TranslateToCultureAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        using HttpClient client = CreateClient(new TranslationResponseHandler(["Olá"], delay: TimeSpan.FromSeconds(5)));
        TranslatorService service = CreateService(client);
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMilliseconds(50));
        Dictionary<string, string> resources = new() { ["Greeting"] = "Hello" };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.TranslateToCultureAsync(resources, "pt-BR", cancellationTokenSource.Token));
    }

    private static TranslatorService CreateService(
        HttpClient client,
        string apiKey = "test-key",
        string endpoint = "https://translation.googleapis.com/")
    {
        var optionsValues = new GoogleTranslateOptions
        {
            ApiKey = apiKey,
            Endpoint = endpoint
        };

        IOptionsFactory<GoogleTranslateOptions> factory = new OptionsFactory<GoogleTranslateOptions>(
            Enumerable.Empty<IConfigureOptions<GoogleTranslateOptions>>(),
            Enumerable.Empty<IPostConfigureOptions<GoogleTranslateOptions>>(),
            Enumerable.Empty<IValidateOptions<GoogleTranslateOptions>>());

        var optionsMonitor = new OptionsMonitor<GoogleTranslateOptions>(
            factory,
            Enumerable.Empty<IOptionsChangeTokenSource<GoogleTranslateOptions>>(),
            new OptionsCache<GoogleTranslateOptions>());

        optionsMonitor.CurrentValue.ApiKey = optionsValues.ApiKey;
        optionsMonitor.CurrentValue.Endpoint = optionsValues.Endpoint;

        return new TranslatorService(client, optionsMonitor, new GoogleTranslateUsageTracker());
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://translation.googleapis.com/")
        };
    }

    private sealed class TranslationResponseHandler(
        IReadOnlyList<string> translations,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        TimeSpan? delay = null,
        object? payload = null) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (delay.HasValue)
            {
                await Task.Delay(delay.Value, cancellationToken);
            }

            if (statusCode != HttpStatusCode.OK)
            {
                return new HttpResponseMessage(statusCode)
                {
                    Content = JsonContent.Create(new { error = "provider failure" })
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload ?? new
                {
                    data = new
                    {
                        translations = translations.Select(value => new { translatedText = value })
                    }
                })
            };
        }
    }
}
