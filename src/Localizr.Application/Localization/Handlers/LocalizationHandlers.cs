using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Abstractions;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Responses;
using Mediator;

namespace Localizr.Application.Localization.Handlers;

/// <summary>Handles localization commands.</summary>
/// <param name="translatorService">The service used to translate resource values.</param>
public sealed class LocalizationHandlers(ITranslatorService translatorService)
    : IRequestHandler<TranslateResourceCommand, Response<TranslateResourceResponse>>
{
    /// <inheritdoc />
    public async ValueTask<Response<TranslateResourceResponse>> Handle(
        TranslateResourceCommand request,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> resources =
            await translatorService.TranslateToCultureAsync(
                request.Resources,
                request.Culture,
                cancellationToken);

        return Response.Success(
            new TranslateResourceResponse(resources, request.Culture));
    }
}
