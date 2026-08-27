namespace Localizr.Application.Localization.Abstractions;

/// <summary>Provides translation operations for normalized localization resources.</summary>
public interface ITranslatorService
{
    /// <summary>Translates resource values to the specified target culture while preserving their keys.</summary>
    /// <param name="resources">The resource entries to translate.</param>
    /// <param name="culture">The target culture name.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A dictionary containing the original keys and translated values.</returns>
    Task<Dictionary<string, string>> TranslateToCultureAsync(
        IReadOnlyDictionary<string, string> resources,
        string culture,
        CancellationToken cancellationToken);
}
