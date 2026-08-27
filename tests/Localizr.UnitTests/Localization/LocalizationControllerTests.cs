using FluentAssertions;
using Localizr.Application.Common.Responses;
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
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(response);

        LocalizationController controller = new(mediator);
        IActionResult result = await controller.Translate(
            new Application.Localization.Requests.TranslateResourceRequest(
                new Dictionary<string, string> { ["Hello"] = "Hello" },
                "pt-BR"),
            CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }
}
