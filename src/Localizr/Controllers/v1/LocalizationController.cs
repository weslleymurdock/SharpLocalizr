using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Requests;
using Localizr.Application.Localization.Responses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Localizr.Controllers.v1;

/// <summary>Exposes localization endpoints.</summary>
/// <param name="mediator">The application mediator.</param>
[ApiController]
public sealed class LocalizationController(IMediator mediator) : ControllerBase
{
    /// <summary>Translates a localization resource to the requested culture.</summary>
    /// <param name="request">The translation request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The translated resource when the operation succeeds.</returns>
    [HttpPost("/localization/translate")]
    [AllowAnonymous]
    [ProducesResponseType<TranslateResourceResponse>(200)]
    [ProducesResponseType<Response<TranslateResourceResponse>>(400)]
    public async Task<IActionResult> Translate(
        TranslateResourceRequest request,
        CancellationToken cancellationToken)
    {
        Response<TranslateResourceResponse> result = await mediator.Send(
            new TranslateResourceCommand(
                request.Resources,
                request.Culture),
            cancellationToken);

        return result.Succeeded
            ? Ok(result.Data)
            : BadRequest(result);
    }
}
