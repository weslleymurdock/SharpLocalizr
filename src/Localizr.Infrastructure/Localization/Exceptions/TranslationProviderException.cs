namespace Localizr.Infrastructure.Localization.Exceptions;

/// <summary>Represents an error returned by an external translation provider.</summary>
public sealed class TranslationProviderException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="TranslationProviderException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code returned by the provider, when available.</param>
    public TranslationProviderException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>Gets the HTTP status code returned by the provider, when available.</summary>
    public int? StatusCode { get; }
}
