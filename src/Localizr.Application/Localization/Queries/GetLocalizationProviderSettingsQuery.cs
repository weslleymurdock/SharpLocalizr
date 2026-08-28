using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Responses;
using Mediator;

namespace Localizr.Application.Localization.Queries;

/// <summary>Requests the current configuration for a localization provider.</summary>
/// <param name="Provider">The provider identifier.</param>
public sealed record GetLocalizationProviderSettingsQuery(
    string Provider) : IRequest<Response<LocalizationProviderSettingsResponse>>;

/// <summary>Requests usage and remaining quota information for a localization provider.</summary>
/// <param name="Provider">The provider identifier.</param>
public sealed record GetLocalizationProviderUsageQuery(
    string Provider) : IRequest<Response<LocalizationProviderUsageResponse>>;
