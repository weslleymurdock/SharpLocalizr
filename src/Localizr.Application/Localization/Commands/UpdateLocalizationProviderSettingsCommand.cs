using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Responses;
using Mediator;

namespace Localizr.Application.Localization.Commands;

/// <summary>Requests an update to a localization provider configuration.</summary>
/// <param name="Provider">The provider identifier.</param>
/// <param name="ApiKey">The provider API key.</param>
public sealed record UpdateLocalizationProviderSettingsCommand(
    string Provider,
    string ApiKey) : IRequest<Response<LocalizationProviderSettingsResponse>>;
