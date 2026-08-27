using FluentAssertions;
using Localizr.Application.Localization.Abstractions;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Handlers;
using Localizr.Application.Localization.Responses;
using NSubstitute;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for localization command handlers.</summary>
public sealed class LocalizationHandlersTests
{
    /// <summary>Verifies that the handler delegates translation and preserves the target culture.</summary>
    [Fact]
    public async Task Handle_WhenTranslationSucceeds_ShouldReturnTranslatedResource()
    {
        ITranslatorService translator = Substitute.For<ITranslatorService>();
        Dictionary<string, string> translated = new() { ["Hello"] = "Olá" };
        translator.TranslateToCultureAsync(
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "pt-BR",
                Arg.Any<CancellationToken>())
            .Returns(translated);

        LocalizationHandlers handler = new(translator);
        TranslateResourceCommand command = new(
            new Dictionary<string, string> { ["Hello"] = "Hello" },
            "pt-BR");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new TranslateResourceResponse(translated, "pt-BR"));
        await translator.Received(1).TranslateToCultureAsync(
            command.Resources,
            command.Culture,
            Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that the handler propagates the cancellation token to the service.</summary>
    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken()
    {
        ITranslatorService translator = Substitute.For<ITranslatorService>();
        Dictionary<string, string> translated = new() { ["Hello"] = "Olá" };
        translator.TranslateToCultureAsync(
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(translated);

        LocalizationHandlers handler = new(translator);
        TranslateResourceCommand command = new(
            new Dictionary<string, string> { ["Hello"] = "Hello" },
            "pt-BR");
        using CancellationTokenSource cancellation = new();
        CancellationToken token = cancellation.Token;

        await handler.Handle(command, token);

        await translator.Received(1).TranslateToCultureAsync(
            command.Resources,
            command.Culture,
            token);
    }
}
