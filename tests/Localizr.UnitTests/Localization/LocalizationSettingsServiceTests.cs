using System.Text.Json;
using Localizr.Infrastructure.Localization.Options;
using Localizr.Infrastructure.Localization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for localization provider settings.</summary>
public sealed class LocalizationSettingsServiceTests
{
    /// <summary>Verifies supported Google settings are returned with a masked API key.</summary>
    [Fact]
    public async Task GetProviderSettingsAsync_WhenGoogleIsConfigured_ShouldReturnMaskedKey()
    {
        using TestConfiguration configuration = CreateConfiguration("abcdefgh1234");
        LocalizationSettingsService service = CreateService(configuration);

        var result = await service.GetProviderSettingsAsync("google", TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Configured);
        Assert.Equal("abcd...1234", result.MaskedApiKey);
    }

    /// <summary>Verifies unsupported providers return no settings.</summary>
    [Fact]
    public async Task GetProviderSettingsAsync_WhenProviderIsUnsupported_ShouldReturnNull()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        LocalizationSettingsService service = CreateService(configuration);

        var result = await service.GetProviderSettingsAsync("unsupported", TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    /// <summary>Verifies a configured API key is persisted to the application settings file.</summary>
    [Fact]
    public async Task UpdateProviderSettingsAsync_WhenGoogleIsSupported_ShouldPersistAndReturnMaskedKey()
    {
        using TestConfiguration configuration = CreateConfiguration("old-key");
        LocalizationSettingsService service = CreateService(configuration);

        var result = await service.UpdateProviderSettingsAsync(
            "google",
            "new-api-key-1234",
            TestContext.Current.CancellationToken);

        Assert.True(result.Configured);
        Assert.Equal("new-...1234", result.MaskedApiKey);

        using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(configuration.FilePath));
        Assert.Equal("new-api-key-1234", document.RootElement
            .GetProperty(GoogleTranslateOptions.SectionName)
            .GetProperty("ApiKey")
            .GetString());
    }

    /// <summary>Verifies unsupported provider updates are rejected.</summary>
    [Fact]
    public async Task UpdateProviderSettingsAsync_WhenProviderIsUnsupported_ShouldThrowArgumentException()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        LocalizationSettingsService service = CreateService(configuration);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateProviderSettingsAsync(
            "unsupported",
            "new-key",
            TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies empty API keys are rejected.</summary>
    [Fact]
    public async Task UpdateProviderSettingsAsync_WhenApiKeyIsEmpty_ShouldThrowArgumentException()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        LocalizationSettingsService service = CreateService(configuration);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateProviderSettingsAsync(
            "google",
            " ",
            TestContext.Current.CancellationToken));
    }

    /// <summary>Verifies cancellation is honored before reading provider settings.</summary>
    [Fact]
    public async Task GetProviderSettingsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        LocalizationSettingsService service = CreateService(configuration);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetProviderSettingsAsync("google", cancellation.Token));
    }

    /// <summary>Verifies supported usage returns the tracked character count and remaining allowance.</summary>
    [Fact]
    public async Task GetProviderUsageAsync_WhenGoogleIsSupported_ShouldReturnUsage()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        GoogleTranslateUsageTracker tracker = new();
        tracker.RecordUsage(1250);
        LocalizationSettingsService service = CreateService(configuration, tracker);

        var result = await service.GetProviderUsageAsync("Google Cloud Translation", TestContext.Current.CancellationToken);

        Assert.True(result.Supported);
        Assert.Equal(1250, result.UsedCharacters);
        Assert.Equal(498750, result.RemainingCharacters);
        Assert.NotNull(result.Message);
    }

    /// <summary>Verifies unsupported usage reports the provider as unsupported.</summary>
    [Fact]
    public async Task GetProviderUsageAsync_WhenProviderIsUnsupported_ShouldReturnUnsupportedResponse()
    {
        using TestConfiguration configuration = CreateConfiguration("test-key");
        LocalizationSettingsService service = CreateService(configuration);

        var result = await service.GetProviderUsageAsync("unsupported", TestContext.Current.CancellationToken);

        Assert.False(result.Supported);
        Assert.Null(result.UsedCharacters);
        Assert.Null(result.RemainingCharacters);
        Assert.NotNull(result.Message);
    }

    private static LocalizationSettingsService CreateService(
        TestConfiguration configuration,
        GoogleTranslateUsageTracker? tracker = null)
    {
        IHostEnvironment environment = Substitute.For<IHostEnvironment>();
        environment.ContentRootPath.Returns(configuration.RootPath);

        GoogleTranslateOptions options = new() { ApiKey = configuration.ApiKey, Endpoint = "https://translation.googleapis.com/" };
        IOptionsFactory<GoogleTranslateOptions> factory = new OptionsFactory<GoogleTranslateOptions>(
            Enumerable.Empty<IConfigureOptions<GoogleTranslateOptions>>(),
            Enumerable.Empty<IPostConfigureOptions<GoogleTranslateOptions>>(),
            Enumerable.Empty<IValidateOptions<GoogleTranslateOptions>>());
        var optionsMonitor = new OptionsMonitor<GoogleTranslateOptions>(
            factory,
            Enumerable.Empty<IOptionsChangeTokenSource<GoogleTranslateOptions>>(),
            new OptionsCache<GoogleTranslateOptions>());
        optionsMonitor.CurrentValue.ApiKey = options.ApiKey;
        optionsMonitor.CurrentValue.Endpoint = options.Endpoint;

        return new LocalizationSettingsService(
            configuration.Configuration,
            environment,
            optionsMonitor,
            tracker ?? new GoogleTranslateUsageTracker());
    }

    private static TestConfiguration CreateConfiguration(string apiKey)
    {
        string root = Path.Combine(Path.GetTempPath(), "SharpLocalizrTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string filePath = Path.Combine(root, "appsettings.json");
        File.WriteAllText(filePath, $$"""{"GoogleTranslate":{"ApiKey":"{{apiKey}}","Endpoint":"https://translation.googleapis.com/"}}""");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        return new TestConfiguration(root, filePath, apiKey, configuration);
    }

    private sealed class TestConfiguration(
        string rootPath,
        string filePath,
        string apiKey,
        IConfigurationRoot configuration) : IDisposable
    {
        public string RootPath { get; } = rootPath;
        public string FilePath { get; } = filePath;
        public string ApiKey { get; } = apiKey;
        public IConfigurationRoot Configuration { get; } = configuration;

        public void Dispose()
        {
            Configuration.Dispose();
            Directory.Delete(RootPath, recursive: true);
        }
    }
}
