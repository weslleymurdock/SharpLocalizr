using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Queries;
using Localizr.Application.Localization.Requests;
using Localizr.Application.Localization.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Localizr.Controllers.v1;

/// <summary>Exposes centralized localization provider settings endpoints.</summary>
/// <param name="mediator">The application mediator.</param>
[ApiController]
[Route("localization/settings")]
public sealed class LocalizationSettingsController(
    IMediator mediator) : ControllerBase
{
    /// <summary>Gets configuration for a localization provider.</summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The provider configuration with sensitive values masked.</returns>
    [HttpGet("{provider}")]
    [ProducesResponseType<Response<LocalizationProviderSettingsResponse>>(200)]
    [ProducesResponseType<Response<LocalizationProviderSettingsResponse>>(404)]
    public async Task<IActionResult> Get(
        string provider,
        CancellationToken cancellationToken)
    {
        Response<LocalizationProviderSettingsResponse> result = await mediator.Send(
            new GetLocalizationProviderSettingsQuery(provider),
            cancellationToken);

        return result.Succeeded
            ? Ok(result)
            : NotFound(result);
    }

    /// <summary>Updates configuration for a localization provider.</summary>
    /// <param name="request">The provider settings request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The updated provider configuration with sensitive values masked.</returns>
    [HttpPut]
    [ProducesResponseType<Response<LocalizationProviderSettingsResponse>>(200)]
    [ProducesResponseType<Response<LocalizationProviderSettingsResponse>>(400)]
    public async Task<IActionResult> Update(
        UpdateLocalizationProviderSettingsRequest request,
        CancellationToken cancellationToken)
    {
        Response<LocalizationProviderSettingsResponse> result = await mediator.Send(
            new UpdateLocalizationProviderSettingsCommand(request.Provider, request.ApiKey),
            cancellationToken);

        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Gets usage and remaining quota information for a localization provider.</summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The provider usage information.</returns>
    [HttpGet("{provider}/usage")]
    [ProducesResponseType<Response<LocalizationProviderUsageResponse>>(200)]
    public async Task<IActionResult> Usage(
        string provider,
        CancellationToken cancellationToken)
    {
        Response<LocalizationProviderUsageResponse> result = await mediator.Send(
            new GetLocalizationProviderUsageQuery(provider),
            cancellationToken);

        return Ok(result);
    }
}
