namespace Localizr.Application.Localization.Abstractions;

/// <summary>Defines an asynchronous writer for localization resource files.</summary>
public interface IResourceWriter
{
    /// <summary>Writes normalized resource entries to a stream.</summary>
    /// <param name="resources">The resource entries to write.</param>
    /// <param name="stream">The destination stream.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync(
        IReadOnlyDictionary<string, string> resources,
        Stream stream,
        CancellationToken cancellationToken);
}
