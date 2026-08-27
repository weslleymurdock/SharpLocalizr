using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Abstractions;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Queries;
using Localizr.Application.Localization.Responses;
using Mediator;

namespace Localizr.Application.Localization.Handlers;

/// <summary>Handles localization provider settings messages.</summary>
/// <param name="settingsService">The localization settings service.</param>
public sealed class LocalizationSettingsHandlers(
    ILocalizationSettingsService settingsService)
    : IRequestHandler<UpdateLocalizationProviderSettingsCommand, Response<LocalizationProviderSettingsResponse>>,
      IRequestHandler<GetLocalizationProviderSettingsQuery, Response<LocalizationProviderSettingsResponse>>,
      IRequestHandler<GetLocalizationProviderUsageQuery, Response<LocalizationProviderUsageResponse>>
{
    /// <inheritdoc />
    public async ValueTask<Response<LocalizationProviderSettingsResponse>> Handle(
        UpdateLocalizationProviderSettingsCommand request,
        CancellationToken cancellationToken)
    {
        LocalizationProviderSettingsResponse result = await settingsService.UpdateProviderSettingsAsync(
            request.Provider,
            request.ApiKey,
            cancellationToken);

        return Response.Success(result);
    }

    /// <inheritdoc />
    public async ValueTask<Response<LocalizationProviderSettingsResponse>> Handle(
        GetLocalizationProviderSettingsQuery request,
        CancellationToken cancellationToken)
    {
        LocalizationProviderSettingsResponse? result = await settingsService.GetProviderSettingsAsync(
            request.Provider,
            cancellationToken);

        return result is null
            ? Response.Failure<LocalizationProviderSettingsResponse>("The requested localization provider is not supported.")
            : Response.Success(result);
    }

    /// <inheritdoc />
    public async ValueTask<Response<LocalizationProviderUsageResponse>> Handle(
        GetLocalizationProviderUsageQuery request,
        CancellationToken cancellationToken)
    {
        LocalizationProviderUsageResponse result = await settingsService.GetProviderUsageAsync(
            request.Provider,
            cancellationToken);

        return Response.Success(result);
    }
}
