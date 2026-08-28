using Localizr.Application.Common.Responses;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Requests;
using Localizr.Application.Localization.Responses;
using Localizr.Controllers.v1;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for the localization controller.</summary>
public sealed class LocalizationControllerTests
{
    /// <summary>Verifies that a successful translation returns the translated resource.</summary>
    [Fact]
    public async Task Translate_WhenTranslationSucceeds_ShouldReturnOk()
    {
        IMediator mediator = Substitute.For<IMediator>();
        Dictionary<string, string> resources = new() { ["Hello"] = "Olá" };
        TranslateResourceResponse response = new(resources, "pt-BR");
        mediator.Send(Arg.Any<TranslateResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Response.Success(response));

        LocalizationController controller = new(mediator);
        IActionResult result = await controller.Translate(
            new TranslateResourceRequest(
                new Dictionary<string, string> { ["Hello"] = "Hello" },
                "pt-BR"),
            TestContext.Current.CancellationToken);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    /// <summary>Verifies that the controller propagates the cancellation token to Mediator.</summary>
    [Fact]
    public async Task Translate_ShouldPropagateCancellationToken()
    {
        IMediator mediator = Substitute.For<IMediator>();
        TranslateResourceResponse response = new(
            new Dictionary<string, string> { ["Hello"] = "Olá" },
            "pt-BR");
        mediator.Send(Arg.Any<TranslateResourceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Response.Success(response));
        LocalizationController controller = new(mediator);
        CancellationToken token = TestContext.Current.CancellationToken;

        await controller.Translate(
            new TranslateResourceRequest(
                new Dictionary<string, string> { ["Hello"] = "Hello" },
                "pt-BR"),
            token);

        await mediator.Received(1).Send(
            Arg.Is<TranslateResourceCommand>(command =>
                command.Culture == "pt-BR" &&
                command.Resources["Hello"] == "Hello"),
            token);
    }
}
