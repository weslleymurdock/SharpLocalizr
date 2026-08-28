namespace Localizr.Application.Localization.Abstractions;

/// <summary>Defines an asynchronous reader for localization resource files.</summary>
public interface IResourceReader
{
    /// <summary>Reads a resource stream into a normalized key/value representation.</summary>
    /// <param name="stream">The stream containing the resource.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The normalized resource entries.</returns>
    Task<IReadOnlyDictionary<string, string>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken);
}
