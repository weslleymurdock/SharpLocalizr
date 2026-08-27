using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Responses;
using Mediator;

namespace Localizr.Application.Localization.Commands;

/// <summary>Requests translation of a localization resource to a target culture.</summary>
/// <param name="Resources">The source resource entries to translate.</param>
/// <param name="Culture">The target culture name.</param>
public sealed record TranslateResourceCommand(
    IReadOnlyDictionary<string, string> Resources,
    string Culture) : IRequest<Response<TranslateResourceResponse>>;
