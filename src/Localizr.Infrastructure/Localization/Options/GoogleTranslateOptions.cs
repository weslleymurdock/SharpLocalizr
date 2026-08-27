namespace Localizr.Infrastructure.Localization.Options;

/// <summary>Provides configuration settings for the Google Cloud Translation Basic API.</summary>
/// <remarks>Store the API key in a secret-management system or environment-specific configuration.</remarks>
public sealed class GoogleTranslateOptions
{
    /// <summary>Gets the configuration section name used for Google Cloud Translation settings.</summary>
    public const string SectionName = "GoogleTranslate";

    /// <summary>Gets or sets the API endpoint used by the Google Cloud Translation Basic API.</summary>
    public string Endpoint { get; set; } = "https://translation.googleapis.com/";

    /// <summary>Gets or sets the API key used to authenticate Google Cloud Translation requests.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
